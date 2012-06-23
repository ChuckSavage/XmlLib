// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using XmlLib.nXPath.Functions;

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{self.Text}")]
    internal class XPath_Part
    {
        internal XPathString self;
        public bool NotEqual { get; private set; }
        public bool Equal { get; private set; }
        public bool LessThan { get; private set; }
        public bool LessThanOrEqual { get { return Equal && LessThan; } }
        public bool GreaterThan { get; private set; }
        public bool GreaterThanOrEqual { get { return Equal && GreaterThan; } }
        public readonly bool ElementAt;
        public readonly FunctionBase Function = null;
        public bool KVP { get; private set; }
        public string Key
        {
            get { return _Key; }
            set { _Key = Format(value).ToString(); }
        }
        string _Key;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (null == _Value)
                {
                    _Value = Format(value.ToString());
                    // isString is set in Format() or IsString()
                    valueIsString = isString;
                }
            }
        }
        object _Value;

        public bool IsValueAttribute { get; private set; }

        public XPath_Part(XPathString text)
        {
            self = text;
            string format = text.Format;
            if (Equal = format.Contains('='))
            {
                string[] parts = format.Split('=');
                Key = parts[0];
                Value = parts[1];

                if (NotEqual = Key.EndsWith("!"))
                    Key = Key.TrimEnd('!');
                else if (LessThan = Key.EndsWith("<"))
                    Key = Key.TrimEnd('<');
                else if (GreaterThan = Key.EndsWith(">"))
                    Key = Key.TrimEnd('>');
                KVP = true;
            }
            else if (LessThan = format.Contains("<"))
            {
                string[] parts = format.Split('<');
                Key = parts[0];
                Value = parts[1];
                KVP = true;
            }
            else if (GreaterThan = format.Contains(">"))
            {
                string[] parts = format.Split('>');
                Key = parts[0];
                Value = parts[1];
                KVP = true;
            }

            {
                Match functionMatch = Regex.Match(format, @"^\w+[^\(]*\([^\)]*\)"); // last(), starts-with(key, value)
                if (functionMatch.Success)
                {
                    string func = functionMatch.Value;
                    string function = func.Split('(')[0];
                    switch (function)
                    {
                        case "ends-with":
                            Function = new EndsWith(this);
                            break;
                        case "last":
                            ElementAt = true;
                            Key = functionMatch.Value;
                            Match digits = Regex.Match(format, @"\d+$"); // ddd
                            int i;
                            if (digits.Success && int.TryParse(digits.Value, out i))
                                _Value = i;
                            else
                                _Value = 0;

                            break;
                        case "local-name":
                            Function = new LocalName(this);
                            break;
                        case "max":
                        case "min":
                            Function = new MinMax(this, func);
                            break;
                        case "name":
                            Function = new Name(this);
                            break;
                        case "string-length":
                            Function = new StringLength(this);
                            break;
                        case "starts-with":
                            Function = new StartsWith(this);
                            break;
                        case "substring":
                            Function = new Substring(this);
                            break;
                    }

                    if (null != Function)
                    {
                        string kvp = Regex.Match(func, @"\(([^\)]*)\)").Groups[1].Value;//.TrimStart('(').TrimEnd(')');
                        if (!string.IsNullOrEmpty(kvp))
                            Function.args = kvp.Split(',').Select(s => Format(s)).ToArray();

                        if (Function.HasKVP)
                        {
                            if (!string.IsNullOrEmpty(kvp) || Function.ArgumentsRequired)
                            {
                                string[] parts = kvp.Split(',');
                                Key = parts[0];
                                try
                                {
                                    Value = parts[1];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    if (Function.ArgumentsValueRequired)
                                        throw new ArgumentException(string.Format(
                                            "Syntax {0}(key, value):  Invalid {0}({1})", func.Split('(')[0], kvp));
                                }
                            }
                            else
                                Key = string.Empty;
                        }
                        else if (Function.args.Length > 0)
                            Key = Function.args[0].ToString();
                        else
                            Key = string.Empty;

                        if (Function.IsEqual)
                            Equal = true;
                    }
                }
            }

            if (null == Key)
            {
                // is this a [0] or [{0}] format?
                int i;
                object value = Format(format);
                if (ElementAt = value is int)
                {
                    Value = (int)value;
                    return;
                }
                if (ElementAt = int.TryParse(format, out i))
                {
                    Value = i;
                    return;
                }
                Key = value.ToString();
            }

            if (IsValueAttribute = Key.StartsWith("@"))
                Key = Key.Substring(1);

            if (Value is string && !valueIsString)
            {
                decimal d;
                if (decimal.TryParse((string)Value, out d))
                    _Value = d;
            }
        }

        /// <summary>
        /// Get the possible Values parameter of the value, aka if its 
        /// a {0} instead of an actual value it needs to be replaced with its value
        /// from the Values list.
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public object Format(string sValue)
        {
            string temp;
            if (IsString(sValue, out temp))
                return temp;
            if (self.Values.Length > 0)
            {
                Match match = Regex.Match(sValue.Trim(), @"^\{(\d+)\}$"); // match "{0}" values only
                if (!match.Success)
                    return sValue;
                var groups = match.Groups;
                //var one = groups[0].Value;
                string number = groups[1].Value;
                int index;
                if (int.TryParse(number, out index) && index < self.Values.Length)
                {
                    object value = self.Values[index];
                    isString = value is string;
                    return value;
                }
            }
            return sValue;
        }

        bool valueIsString = false, isString = false;
        bool IsString(string value, out string newValue)
        {
            newValue = value;
            // have quoted value
            Match quotes = Regex.Match(value, "'.*'"); // greedy .* so that can have quotes within the quote
            if (quotes.Success)
            {
                newValue = value.Replace("'", "");
                return isString = true;
            }
            return false;
        }
    }
}
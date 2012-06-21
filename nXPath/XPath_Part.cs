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
        public string Key { get; private set; }
        public object Value
        {
            get { return _Value; }
            set
            {
                if (null == _Value)
                    _Value = value;
            }
        }
        object _Value;

        public bool IsValueAttribute { get; private set; }

        public XPath_Part(XPathString text)
        {
            bool isString = false;
            self = text;
            string part = text.Text;
            if (text.Values.Length > 0)
            {
                Value = text.Values[0];
                isString = Value is string;
            }
            else
            {
                int i;
                if (int.TryParse(part, out i))
                {
                    Value = i;
                    ElementAt = true;
                    return;
                }
                // have quoted value
                isString = IsString(part, out part);
            }

            Key = part;
            if (Equal = part.Contains('='))
            {
                string[] parts = part.Split('=');
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
            else if (LessThan = part.Contains("<"))
            {
                string[] parts = part.Split('<');
                Key = parts[0];
                Value = parts[1];
                KVP = true;
            }
            else if (GreaterThan = part.Contains(">"))
            {
                string[] parts = part.Split('>');
                Key = parts[0];
                Value = parts[1];
                KVP = true;
            }
            
            {
                Match functionMatch = Regex.Match(part, @"^\w+[^\(]*\([^\)]*\)"); // last(), starts-with(key, value)
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
                            Match digits = Regex.Match(part, @"\d+$"); // ddd
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
                    }

                    if (null != Function)
                    {
                        if (Function.HasKVP)
                        {
                            string kvp = Regex.Match(func, @"\(([^\}]*)\)").Value.TrimStart('(').TrimEnd(')');
                            if (!string.IsNullOrEmpty(kvp) || Function.ArgumentsRequired)
                            {
                                string[] parts = kvp.Split(',');
                                Key = parts[0];
                                string value;
                                try
                                {
                                    if (isString) // had form "node[func(@key, 'value')]"
                                        Value = parts[1];
                                    else
                                    {
                                        isString = IsString(parts[1], out value);
                                        Value = value;
                                    }
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
                        if (Function.IsEqual)
                            Equal = true;
                    }
                }
            }

            if (IsValueAttribute = Key.StartsWith("@"))
                Key = Key.Substring(1);

            if (Value is string && !isString)
            {
                decimal d;
                if (decimal.TryParse((string)Value, out d))
                    _Value = d;
            }
        }

        bool IsString(string value, out string newValue)
        {
            newValue = value;
            // have quoted value
            Match quotes = Regex.Match(value, "'.*'"); // greedy .* so that can have quotes within the quote
            if (quotes.Success)
            {
                newValue = value.Replace("'", "");
                return true;
            }
            return false;
        }
    }
}

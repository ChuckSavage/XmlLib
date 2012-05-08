// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

#if SeaRisenLib2
namespace SeaRisenLib2.Xml
#else
namespace XmlLib
#endif
{
    public class XPathParser
    {
        class Bracket
        {
            string self, key, value;
            public readonly bool equal, greater, less, isStringValue;

            public Bracket(string part)
            {
                value = self = part = part.TrimStart('[').TrimEnd(']');
                // have quoted value
                Match quotes = Regex.Match(self, "='.*'"); // "='[^]]*'" - works but isn't greedy
                if (quotes.Success)
                {
                    isStringValue = true;
                    part = part.Replace("'", "");
                }

                key = part;
                if (equal = part.Contains('='))
                {
                    string[] parts = part.Split('=');
                    key = parts[0];
                    value = parts[1];

                    if (less = key.EndsWith("<"))
                        key = key.TrimEnd('<');
                    if (greater = key.EndsWith(">"))
                        key = key.TrimEnd('>');
                }
                else if (less = key.Contains("<"))
                {
                    string[] parts = part.Split('<');
                    key = parts[0];
                    value = parts[1];
                }
                else if (greater = key.Contains(">"))
                {
                    string[] parts = part.Split('>');
                    key = parts[0];
                    value = parts[1];
                }
            }

            #region Decimal

            public decimal Decimal
            {
                get
                {
                    if (decimal.MinValue == _Decimal)
                        _Decimal = decimal.Parse(value);
                    return _Decimal;
                }
            }
            decimal _Decimal = decimal.MinValue;

            public bool IsDecimal 
            {
                get
                {
                    if (!isStringValue)
                    {
                        decimal d;
                        if (decimal.TryParse(self, out d))
                        {
                            _Decimal = d;
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            public IEnumerable<XElement> Elements(IEnumerable<XElement> elements)
            {
                if (IsDecimal)
                {
                    int index = (int)(Decimal - 1);
                    XElement e;
                    if (index > 1)
                        e = elements.ElementAt(index);
                    else
                        e = elements.First();
                    elements = new XElement[] { e };
                }
                else
                    elements = elements.Where(WhereFunction);
                return elements;
            }

            private bool IsMatch(string str)
            {
                bool match = false;
                if (equal)
                    match = IsEqual(str);
                if (less)
                    match |= IsLess(str);
                else
                    if (greater)
                        match |= IsGreater(str);
                return match;
            }

            private bool WhereFunction(XElement x)
            {
                if (key.StartsWith("@"))
                {
                    //key = key.TrimStart(
                    var a = x.Attribute(key.TrimStart('@'));
                    if (null == a) return false;
                    return IsMatch(a.Value);
                }
                return x.Elements(key).Any(xx => IsMatch(xx.Value));
            }

            public string Key { get { return key; } }

            private bool IsEqual(string a)
            {
                if (null == a && null == value) return true;
                if (null == a) return false;

                if (isStringValue)
                    return a == value;

                decimal c;
                if (decimal.TryParse(a, out c))
                    return c == Decimal;
                return false;
            }

            private bool IsLess(string a)
            {
                if (null == a) return false;

                if (isStringValue)
                    return string.Compare(a, value) < 0;

                decimal c;
                if (decimal.TryParse(a, out c))
                    return c < Decimal;
                return false;
            }

            private bool IsGreater(string a)
            {
                if (null == a) return false;

                if (isStringValue)
                    return string.Compare(a, value) > 0;

                decimal c;
                if (decimal.TryParse(a, out c))
                    return c > Decimal;
                return false;
            }

            /// <summary>
            /// If we're comparing value's by string
            /// </summary>
            private bool IsValueString(string value, out string result)
            {
                result = value;
                if (value.Contains("'") || value.Contains("\""))
                {
                    result = value.Replace("\"", string.Empty)
                                 .Replace("'", string.Empty);
                    return true;
                }
                return false;
            }

            public string Value { get { return value; } }

            public string XPath { get { return self; } }
        }
        XElement source;

        public XPathParser(XElement source)
        {
            if (null == source)
                throw new ArgumentNullException("XElement cannot be null to constructor.");
            this.source = source;
        }

        private Bracket[] GetBrackets(string part, out string name)
        {
            name = part;
            if (part.Contains('['))
            {
                Match[] matches = Regex.Matches(part, @"\[[^]]*\]")
                       .Cast<Match>()
                       .ToArray();
                Bracket[] result = matches
                       .Select(match => new Bracket(match.Value))
                       .ToArray();
                if (matches.Length > 0)
                    name = part.Remove(matches[0].Index);
                return result;
            }
            return null;
        }

        private XElement ParseInternal(XElement contextNode, string name, Bracket[] brackets)
        {
            var elements = contextNode.GetElements(name);
            if(null != brackets)
                foreach (Bracket bracket in brackets)
                {
                    elements = bracket.Elements(elements);
                }
            return elements.FirstOrDefault();
        }

        public XElement Parse(string path, bool create)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null to Parse method.");

            string[] parts = path.Split('\\', '/');
            XElement result = source;
            foreach (string _part in parts)
            {
                string part = _part;
                if (part.Contains('['))
                {
                    string name;
                    var brackets = GetBrackets(part,out name);
                    XElement temp = ParseInternal(result, name, brackets);
                    if (null != temp)
                    {
                        result = temp;
                        continue;
                    }
                    part = part.Split('[')[0];
                }
                if (create)
                    result = result.GetElement(part);
                else
                {
                    result = result.Element(result.ToXName(part));
                    if (null == result)
                        throw new ArgumentOutOfRangeException(part);
                }
            }
            return result;
        }

        public static XElement Parse(XElement source, string path, params string[] args)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null to Parse method.");
            return Parse(source, string.Format(path, args));
        }

        public static XElement Parse(XElement source, string path)
        {
            return new XPathParser(source).Parse(path, true);
        }
    }
}
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
            public readonly bool equal, greater, less, isStringValue, kvp;

            public Bracket(string part)
            {
                value = self = part = part.TrimStart('[').TrimEnd(']');
                // have quoted value
                Match quotes = Regex.Match(self, "='.*'"); // greedy .* so that can have quotes within the quote
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
                    kvp = true;
                }
                else if (less = key.Contains("<"))
                {
                    string[] parts = part.Split('<');
                    key = parts[0];
                    value = parts[1];
                    kvp = true;
                }
                else if (greater = key.Contains(">"))
                {
                    string[] parts = part.Split('>');
                    key = parts[0];
                    value = parts[1];
                    kvp = true;
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
                else if ("*" != self)
                {
                    if (key.StartsWith("last()"))
                    {
                        if ("last()" == key)
                            return new XElement[] { elements.Last() };
                        int i;
                        if (int.TryParse(key.Remove(0, "last()-".Length), out i))
                            return new XElement[] { elements.ElementAt(elements.Count() - (i + 1)) };
                        throw new ArgumentException(self);
                    }
                    else
                        elements = elements.Where(Elements_WhereFunction);
                }
                return elements;
            }

            // separate function instead of inline .Where(x => ...) so can use debugger.
            private bool Elements_WhereFunction(XElement x)
            {
                if (kvp)
                {
                    if (key.StartsWith("@"))
                    {
                        //key = key.TrimStart(
                        var a = x.Attribute(key.TrimStart('@'));
                        if (null == a) return false;
                        return IsMatch(a.Value);
                    }
                    return x.GetElements(key).Any(xx => IsMatch(xx.Value));
                }
                if (key.StartsWith("@"))
                    return null != x.Attribute(key.TrimStart('@'));
                return x.GetElements(key).Count() > 0;
            }

            /// <summary>
            /// The left side of an expression.
            /// <remarks>Aka: para[key=value]</remarks>
            /// </summary>
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
            /// Check if string is a match for a Where() clause.
            /// </summary>
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

            /// <summary>
            /// The right side of an expression.
            /// <remarks>Aka: para[key=value]</remarks>
            /// </summary>
            public string Value { get { return value; } }

            /// <summary>
            /// The full expression.
            /// </summary>
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

        private IEnumerable<XElement> ParseInternal(XElement contextNode, string part)
        {
            string name;
            Bracket[] brackets = GetBrackets(part, out name);
            var elements = contextNode.GetElements(name);
            if(null != brackets)
                foreach (Bracket bracket in brackets)
                {
                    elements = bracket.Elements(elements);
                }
            return elements;
        }

        public IEnumerable<XElement> ParseEnumerable(string path, bool create)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null.");

            string[] parts = path.Split('\\', '/');
            XElement result = source;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool last = (i + 1) == parts.Length;
                if (part.Contains('['))
                {
                    var e = ParseInternal(result, part);
                    if (last)
                        return e;

                    XElement temp = e.FirstOrDefault();
                    if (null != temp)
                    {
                        result = temp;
                        continue;
                    }
                    part = part.Split('[')[0];
                }
                if (last)
                    return result.GetElements(part);
                if (create)
                    result = result.GetElement(part);
                else
                {
                    result = result.Element(result.ToXName(part));
                    if (null == result)
                        throw new ArgumentOutOfRangeException(part);
                }
            }
            return new XElement[] { result };
        }

        public XElement Parse(string path, bool create)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null.");

            return ParseEnumerable(path, create).FirstOrDefault();
        }

        public static XElement Parse(XElement source, string path, params string[] args)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null to Parse method.");
            return Parse(source, string.Format(path, args));
        }

        public static XElement Parse(XElement source, string path, bool create)
        {
            return new XPathParser(source).Parse(path, create);
        }

        public static IEnumerable<XElement> ParseEnumerable(XElement source, string path, bool create)
        {
            return new XPathParser(source).ParseEnumerable(path, create);
        }
    }
}
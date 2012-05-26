// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{Text}")]
    public class XPathString
    {
        /// <summary>
        /// Create elements if they don't exist so as not to throw null exceptions.
        /// </summary>
        public bool Create = true;
        /// <summary>
        /// The text of the path will filled in values.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// The string.Format version of the path.
        /// </summary>
        public readonly string Format;
        /// <summary>
        /// The values to fill the Format with for string.Format.
        /// </summary>
        public readonly object[] Values;
        /// <summary>
        /// Elements() or Descendants() to be called with the Name, 
        /// aka the path was proceeded with two slashes.
        /// </summary>
        public readonly bool IsElements;
        /// <summary>
        /// Does the path contain brackets.
        /// This is weak-sauce, everything should be treated as an XPath for more advanced XPath scenarios.
        /// </summary>
        public bool IsXPath { get { return Text.Contains('['); } }
        /// <summary>
        /// The breakdown in the path into its separate path segments.
        /// Aka breaking it up via the forward-slash segments.
        /// <para>Example: segement1[stillSegment1/1/1]/segment2[2/2=2]/segment3</para>
        /// </summary>
        public readonly XPathString[] PathSegments;

        /// <summary>
        /// "pair[@Key&gt;={0} and @Key&lt;{1}]", 4, 6
        /// </summary>
        public XPathString(string path, params object[] values)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("XPath cannot be null or empty!");
            if (null == values)
                values = new object[] { };
            if (path.StartsWith("//"))
            {
                IsElements = false; // is Descendants
                path = path.Substring("//".Length);
            }
            else
                IsElements = true;
            Format = path;
            Text = string.Format(path, values);
            Values = values;

            PathSegments = Split();
        }

        internal XPath_Bracket[] Brackets
        {
            get { return _Brackets ?? (_Brackets = GetBrackets(out _Name)); }
        }
        XPath_Bracket[] _Brackets;

        /// <summary>
        /// Combine two or more XPathString's into one.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static XPathString Combine(IEnumerable<XPathString> list) { return Combine(list.ToArray()); }

        /// <summary>
        /// Combine two or more XPathString's into one.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static XPathString Combine(params XPathString[] list)
        {
            if (null == list)
                throw new ArgumentNullException("List cannot be null");
            if (0 == list.Length)
                throw new ArgumentException("List cannot be empty");
            if (1 == list.Length)
                return list[0];

            StringBuilder format = new StringBuilder(list[0].Format);
            List<object> values = list[0].Values.ToList();
            int index = values.Count;
           
            foreach (XPathString xp in list.Skip(1))
            {
                string xpFormat = xp.Format;
                MatchCollection ms = Regex.Matches(xpFormat, ToPaths_Pattern);
                if (ms.Count > 0)
                {
                    for (int j = 0; j < ms.Count; j++)
                    {
                        Match match = ms[j];
                        string s = string.Format("{{{0}}}", j + index);
                        xpFormat = xpFormat.Replace(match.Value, s);
                    }
                    index += ms.Count;
                    values.AddRange(xp.Values);
                }
                format.Append("/" + xpFormat);
            }
            return new XPathString(format.ToString(), values.ToArray());
        }

        private XPath_Bracket[] GetBrackets(out string name)
        {
            name = Text;
            if (Text.Contains('['))
            {
                Match[] matches = Regex.Matches(Format, @"\[[^]]*\]")
                       .Cast<Match>()
                       .ToArray();
                if (matches.Length > 0)
                {
                    name = Format.Remove(matches[0].Index);
                    List<string> list = new List<string>() { name };
                    list.AddRange(matches.Select(m => m.Value));
                    XPathString[] parts = ToPaths(list);
                    XPath_Bracket[] result = parts
                           .Skip(1)
                           .Select(xps => new XPath_Bracket(xps))
                           .ToArray();
                    return result;
                }
            }
            return new XPath_Bracket[] { };
        }

        /// <summary>
        /// The name of the node's that will be returned from the search if successful.
        /// </summary>
        public string Name
        {
            get
            {
                if (null == _Name)
                    _Brackets = GetBrackets(out _Name);
                return _Name;
            }
        }
        string _Name;

        /// <summary>
        /// Split the path into its separate XPathStrings.
        /// </summary>
        public XPathString[] Split() { return ToPaths(SplitInternal()); } 

        /*
         * Split by '/' but not when '/' is with '[]' square brackets
         */
        private string[] SplitInternal()
        {
            return Regex.Matches(Format, @"([^/\[\]]|\[[^]]*\])+")
                                    .Cast<Match>()
                                    .Select(m => m.Value)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToArray();
        }

        /// <summary>
        /// Returns a XPathString array with the associated values in each XPathString.
        /// </summary>
        public XPathString[] Split(params char[] separator)
        {
            return ToPaths(Format.Split(separator));
        }

        /// <summary>
        /// Returns a XPathString array with the associated values in each XPathString.
        /// </summary>
        public XPathString[] Split(string[] separator, StringSplitOptions option)
        {
            return ToPaths(Format.Split(separator, option));
        }

        const string ToPaths_Pattern = @"\{(\d+):?\w*\}";  // "ggg[xcc={0}]/aa[xx={1}][fdjskl={2}]"

        /// <summary>
        /// Split out separate XPathString's by parts of an array that concatted equals Text.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public XPathString[] ToPaths(params string[] parts)
        {
            if (null == parts)
                throw new ArgumentNullException("parts");
            if (parts.Length > 1)
            {
                int index = 0;
                List<XPathString> list = new List<XPathString>();
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (string.IsNullOrEmpty(part))
                        continue;
                    List<object> args = new List<object>();
                    MatchCollection ms = Regex.Matches(part, ToPaths_Pattern);
                    if (ms.Count > 0)
                    {
                        args.AddRange(Values.Skip(index).Take(ms.Count));
                        for (int j = 0; j < ms.Count; j++)
                        {
                            Match match = ms[j];
                            string s = string.Format("{{{0}}}", j);
                            part = part.Replace(match.Value, s);
                        }
                        index += ms.Count;
                    }
                    list.Add(new XPathString(part, args.ToArray()));
                }
                return list.ToArray();
            }
            return new[] { this };
        }

        public XPathString[] ToPaths(IEnumerable<string> parts)
        {
            return ToPaths(parts.ToArray());
        }

        #region Comparing

        public static IComparer Comparer { get { return new _Comparer(); } }

        private class _Comparer : IComparer<XPathString>, IComparer
        {
            public int Compare(XPathString x, XPathString y)
            {
                if (null == x && null == y) return 0;
                if (null == x) return -1;
                if (null == y) return 1;
                return string.Compare(x.Text, y.Text);
            }

            public int Compare(object x, object y)
            {
                return Compare(x as XPathString, y as XPathString);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is XPathString)
                return Text == ((XPathString)obj).Text;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        #endregion

        public override string ToString()
        {
            return Text;
        }
    }
}

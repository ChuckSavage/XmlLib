// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{Text}")]
    public class XPathString
    {
        public readonly string Text;
        public readonly string Format;
        public readonly object[] Values;
        public readonly bool IsElements;

        public bool IsXPath { get { return Text.Contains('['); } }

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

        /// <summary>
        /// Split the path into its separate XPathStrings.
        /// </summary>
        public XPathString[] Split() { return ToPaths(SplitInternal()); }

        /*
         * Split by '/' but not when '/' is with '[]' square brackets
         */
        private string[] SplitInternal()
        {
            // Regex test: http://regexpal.com/
            //string s = "pair[@Key=2]/Items/Item[Person/Name='Martin']/Date";
            //string[] result = Regex.Split(s, @"(?<!\[[^]]+)/"); // < this works
            return Regex.Matches(Format, @"([^/\[\]]|\[[^]]*\])+")
                                    .Cast<Match>()
                                    .Select(m => m.Value)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToArray();
            /* Original I created before getting good responses on StackOverflow
            List<string> list = new List<string>();
            int pos = 0, i = 0;
            bool within = false;
            Func<string> add = () => Format.Substring(pos, i - pos);
            //string a;
            for (; i < Format.Length; i++)
            {
                //a = add();
                char c = Format[i];
                switch (c)
                {
                    case '/':
                        if (!within)
                        {
                            list.Add(add());
                            pos = i + 1;
                        }
                        break;
                    case '[':
                        within = true;
                        break;
                    case ']':
                        within = false;
                        break;
                }
            }
            list.Add(add());
            return list.Where(s => !string.IsNullOrEmpty(s)).ToArray();
             */
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
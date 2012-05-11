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

            PathSegments = ToPaths(path.Split('\\', '/'));
        }

        const string Pattern = @"\{(\d+):?\w*\}";  // "ggg[xcc={0}]/aa[xx={1}][fdjskl={2}]"

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
                    MatchCollection ms = Regex.Matches(part, Pattern);
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
    }
}

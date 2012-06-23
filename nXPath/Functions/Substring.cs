// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    /// <summary>
    /// <para>Example: root2.XPathElement("pair[substring()={0}]", 5);</para>
    /// <para>Syntax: string-length(nodeset, start)</para>
    /// <para>Syntax: string-length(nodeset, start, length)</para>
    /// <para>- nodeset: path to a node or attribute, and its value.</para>
    /// <para>- start: int value starting with 1 for first character.</para>
    /// <para>- length: int value for last character.</para>
    /// <para>Returns: Evaulates to true or false, whether the string is equal to the value</para>
    /// </summary>
    internal class Substring : FunctionBase
    {
        public Substring(XPath_Part part)
            : base(part, typeof(Substring_Generic<>))
        {
        }

        /// <summary>
        /// false (isn't IsEqual exclusively)
        /// </summary>
        internal override bool IsEqual
        {
            get { return false; }
        }

        /// <summary>
        /// false
        /// </summary>
        internal override bool HasKVP
        {
            get { return false; }
        }

        internal class Substring_Generic<T> : GenericBase
        {
            Substring self;
            string key;
            int start, length = -1;

            public Substring_Generic(Substring parent, XElement nodeToCheck)
                : base(nodeToCheck, parent.part)
            {
                self = parent;
            }

            //int Convert<TT>(TT s) { return ConvertX<TT, int>.ToValue(s); }

            public override void Init()
            {
                base.Init();

                if (self.args.Length < 2)
                    throw new ArgumentException("Insufficient arguments to substring function");
                key = self.args[0].ToString();
                start = Convert.ToInt32(self.args[1]) - 1; // xpath is 1 for first character, c# is 0 for first character
                if (self.args.Length > 2)
                    length = Convert.ToInt32(self.args[2]);
            }

            string substring(string value)
            {
                if (-1 == length || ((start + length) > value.Length))
                    if (start >= value.Length)
                        return string.Empty;
                    else
                        return value.Substring(start);
                return value.Substring(start, length);
            }

            public override bool Eval()
            {
                try
                {
                    //var part = self.part;

                    //if (!node.HasElements && node.Value.Contains("33"))
                    //    System.Diagnostics.Debugger.Break();

                    List<string> values = new List<string>();
                    if (string.IsNullOrEmpty(key))
                        values.Add(substring(node.Name.LocalName));
                    else
                    {
                        string[] sArray;
                        if (!nodeset.NodeValue(node, out sArray))
                            return false;
                        values.AddRange(sArray.Select(s => substring(s)));
                    }
                    return KVP.Eval(part, values.ToArray());
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                return false;
            }
        }
    }
}
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
    /// <para>Example: root2.XPathElement("pair[string-length()={0}]", 5);</para>
    /// <para>Syntax: string-length()</para>
    /// <para>Syntax: string-length(nodeset to string node)</para>
    /// <para>- nodeset: path to a node or attribute, and its local-name.</para>
    /// <para>Returns: Evaulates to true or false, whether the string is equal to the value</para>
    /// </summary>
    internal class StringLength : FunctionBase
    {
        public StringLength(XPath_Part part)
            : base(part, typeof(StringLength_Generic<>))
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
        internal override bool ArgumentsRequired
        {
            get { return false; }
        }

        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsValueRequired
        {
            get { return false; }
        }

        internal class StringLength_Generic<T> : GenericBase
        {
            public StringLength_Generic(StringLength name, XElement nodeToCheck)
                : base(nodeToCheck, name.part)
            {
            }

            T Convert(string s) { return ConvertX<int, T>.ToValue(s.Length); }

            public override bool Eval()
            {
                try
                {
                    List<T> values = new List<T>();
                    if (string.IsNullOrEmpty(part.Key))
                        values.Add(Convert(node.Name.LocalName));
                    else
                    {
                        string[] sArray;
                        if (!nodeset.NodeValue(node, out sArray))
                            return false;
                        values.AddRange(sArray.Select(s => Convert(s)));
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
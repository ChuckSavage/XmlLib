// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    /*
     * Syntax: Contains(,'x') - tag-name contains 'x'
     * Syntax: Contains(nodeset, 'x') - nodeset contains 'x'
     * Syntax: Contains(nodeset, 'x', (bool)ConvertToUpper/ElseLower)
     * Syntax: Contains('x', nodeset) - 'x' contains nodeset
     * Syntax: Contains('x', nodeset, (bool)ConvertToUpper/ElseLower)
     */
    internal class Contains : FunctionBase
    {
        internal Contains(XPath_Part part) : base(part, typeof(Contains_Generic<>)) { }

        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal class Contains_Generic<T> : GenericBase
        {
            Contains self;
            bool? isUpper = false;
            string pattern;

            public Contains_Generic(Contains parent)
                : base(parent.part)
            {
                self = parent;
            }

            bool Eval(string value)
            {
                if (true == isUpper)
                    value = value.ToUpperInvariant();
                else if (false == isUpper)
                    value = value.ToLowerInvariant();
                if (null != self.NodeSet && 1 == self.NodeSet.Index)
                    return pattern.Contains(value);
                return value.Contains(pattern);
            }

            public override bool Eval(XElement node)
            {
                try
                {
                    if ("" == part.Key)
                        return Eval(node.Name.LocalName);
                    T[] values;
                    if (nodeset.NodeValue(node, out values) && values.Length > 0)
                        return values.Any(v => Eval(v.ToString()));
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                return false;
            }

            public override void Init()
            {
                if (self.Args.Length > 2)
                {
                    object arg = self.Args[2];
                    if (arg is bool)
                        isUpper = (bool)arg;
                    else if (arg is string)
                    {
                        bool temp;
                        if (bool.TryParse(arg.ToString(), out temp))
                            isUpper = temp;
                    }
                }

                pattern = part.Value.ToString();
                if (true == isUpper)
                    pattern = pattern.ToUpperInvariant();
                else if (false == isUpper)
                    pattern = pattern.ToLowerInvariant();
            }
        }
    }
}
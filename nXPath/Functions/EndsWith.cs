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
    internal class EndsWith : FunctionBase
    {
        internal EndsWith(XPath_Part part) : base(part, typeof(EndsWithGeneric<>)) { }

        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal class EndsWithGeneric<T> : GenericBase
        {
            public EndsWithGeneric(EndsWith ends, XElement nodeToCheck)
                : base(nodeToCheck, ends.part)
            {
            }

            public override bool Eval()
            {
                try
                {
                    string endsWith = part.Value.ToString();
                    T[] values;
                    if (nodeset.NodeValue(node, out values))
                        return values.Any(v => v.ToString().EndsWith(endsWith));
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
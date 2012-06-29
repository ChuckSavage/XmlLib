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
    /// <summary>
    /// <para>Example: root.XPath("//*[ends-with(first, second)]");</para>
    /// <para>Syntax: ends-with(,'x')</para>
    /// <para>Syntax: ends-with(nodeset, 'x')</para>
    /// <para>Syntax: ends-with(nodeset, 'x', (bool)ConvertToUpper/ElseLower)</para>
    /// <para>Syntax: ends-with('x', nodeset)</para>
    /// <para>Syntax: ends-with('x', nodeset, (bool)ConvertToUpper/ElseLower)</para>
    /// <para>- nodeset: path to a node or attribute and its value</para>
    /// <para>Returns: Returns the nodes that first ends-with second.</para>
    /// </summary>
    internal class EndsWith : FunctionBase
    {
        internal EndsWith(XPath_Part part) : base(part, typeof(EndsWithGeneric<>)) { }

        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal class EndsWithGeneric<T> : StringGeneric<T>
        {
            public EndsWithGeneric(EndsWith parent)
                : base(Eval, parent)
            {
            }

            static bool Eval(string a, string b)
            {
                return a.EndsWith(b);
            }
        }
    }
}
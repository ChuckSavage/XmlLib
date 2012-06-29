// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

namespace XmlLib.nXPath.Functions
{
    /// <summary>
    /// <para>Example: root.XPath("//*[contains(first, second)]");</para>
    /// <para>Syntax: Contains(,'x') - tag-name contains 'x'</para>
    /// <para>Syntax: Contains(nodeset, 'x') - nodeset contains 'x'</para>
    /// <para>Syntax: Contains(nodeset, 'x', (bool)ConvertToUpper/ElseLower)</para>
    /// <para>Syntax: Contains('x', nodeset) - 'x' contains nodeset</para>
    /// <para>Syntax: Contains('x', nodeset, (bool)ConvertToUpper/ElseLower)</para>
    /// <para>- nodeset: path to a node or attribute and its value</para>
    /// <para>Returns: Returns the nodes that first contains second.</para>
    /// </summary>
    internal class Contains : FunctionBase
    {
        internal Contains(XPath_Part part) : base(part, typeof(Contains_Generic<>)) { }

        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal class Contains_Generic<T> : StringGeneric<T>
        {
            public Contains_Generic(Contains parent)
                : base(Eval, parent)
            {
            }

            static bool Eval(string a, string b)
            {
                return a.Contains(b);
            }
        }
    }
}
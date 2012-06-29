// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

namespace XmlLib.nXPath.Functions
{
    /// <summary>
    /// <para>Example: root.XPath("//*[starts-with(first, second)]");</para>
    /// <para>Syntax: starts-with(,'x')</para>
    /// <para>Syntax: starts-with(nodeset, 'x')</para>
    /// <para>Syntax: starts-with(nodeset, 'x', (bool)ConvertToUpper/ElseLower)</para>
    /// <para>Syntax: starts-with('x', nodeset)</para>
    /// <para>Syntax: starts-with('x', nodeset, (bool)ConvertToUpper/ElseLower)</para>
    /// <para>- nodeset: path to a node or attribute and its value</para>
    /// <para>Returns: Returns the nodes that first starts-with second.</para>
    /// </summary>
    internal class StartsWith : FunctionBase
    {
        internal StartsWith(XPath_Part part) : base(part, typeof(StartsWithGeneric<>)) { }

        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal class StartsWithGeneric<T> : StringGeneric<T>
        {
            public StartsWithGeneric(StartsWith parent)
                : base(Eval, parent)
            {
            }

            static bool Eval(string a, string b)
            {
                return a.StartsWith(b);
            }
        }
    }
}
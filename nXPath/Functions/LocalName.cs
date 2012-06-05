// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    /// <summary>
    /// <para>Example: root2.XPathElement("pair[local-name()={0}]", "div");</para>
    /// <para>Syntax: local-name()</para>
    /// <para>Syntax: local-name(nodeset)</para>
    /// <para>- nodeset: path to a node or attribute, and its local-name.</para>
    /// <para>Returns: Returns the name of the current node or the first node 
    /// in the specified node set - without the namespace prefix</para>
    /// </summary>
    internal class LocalName : FunctionBase
    {
        public LocalName(XPath_Part part)
            : base(part)
        {
        }

        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsRequired { get { return false; } }
        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsValueRequired { get { return false; } }

        internal override Expression Left(XPath_Part part, Expression left, Expression right, Expression path)
        {
            //ParameterExpression pe = Expression.Parameter(typeof(XElement), "any");
            Expression name = Expression.Property(XPath_Bracket.pe, "Name");
            Expression localName = Expression.Property(name, "LocalName");
            return localName;
        }
    }
}
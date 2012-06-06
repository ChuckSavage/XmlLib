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
    /// <para>Example: root2.XPathElement("pair[max(@Key, {0})]", 0);</para>
    /// <para>Syntax: max(key, type)</para>
    /// <para>- key: is element or attribute path nodeset</para>
    /// <para>- type: isn't a value to compare against, but the type of the key to get 
    /// the min/max value of.</para>
    /// <para>Returns: The min/max pair node.</para>
    /// </summary>
    internal class MinMax : FunctionBase
    {
        enum eMinMax
        {
            Min, Max
        }
        readonly eMinMax Function;

        internal MinMax(XPath_Part part, string function)
            : base(part)
        {
            if (function.StartsWith("min"))
                Function = eMinMax.Min;
            else
                Function = eMinMax.Max;
            _CompareElement = MinMax_CompareElement;
        }

        internal Expression MinMax_CompareElement(XPath_Part part, string key, Expression parent, Expression left, Expression right)
        {
            left = XPathUtils.ElementValue(parent, part, key);
            return XPath_Bracket.ExpressionEquals(part, left, right, null);
        }

        internal override Expression Right(XPath_Part part, Expression left, Expression right, Expression path)
        {
            // right = x.Parent.Elements(x.Name).Max(xx => (int)xx.Attribute("Key")
            string minmax = ((MinMax)part.Function).Function.ToString();
            string[] keyParts = part.Key.Split('/');
            string key = keyParts.Last();
            ParameterExpression maxPe = Expression.Parameter(typeof(XElement), minmax.ToLower());
            Expression parent = Expression.Property(XPath_Bracket.pe, "Parent");
            Expression value;
            if (keyParts.Length > 1)
                path = Expression.Call(
                        typeof(XElementExtensions),
                        "GetElement",
                        null,
                        maxPe,
                        Expression.Constant(string.Join("/", keyParts.Take(keyParts.Length - 1).ToArray()))
                        );

            if (part.IsValueAttribute)
                value = XPathUtils.AttributeValue(path ?? maxPe, part, key);
            else
                value = XPathUtils.ElementValue(path ?? maxPe, part, key);
            
            Expression name = Expression.Property(XPath_Bracket.pe, "Name");
            Expression elements = Expression.Call(
                    typeof(XElementExtensions),
                    "GetElements",
                    null,
                    parent,
                    name
                    );
            right = Expression.Call(
                    typeof(Enumerable),
                    minmax,
                    new[] { typeof(XElement), part.Value.GetType() },
                    elements,
                    Expression.Lambda(value, new ParameterExpression[] { maxPe }));
            return right;
        }
    }
}

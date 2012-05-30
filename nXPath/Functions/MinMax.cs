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
    public class MinMax
    {
        internal static Expression Parse(XPath_Part part, Expression left, Expression right, Expression path)
        {
            // right = x.Parent.Elements(x.Name).Max(xx => (int)xx.Attribute("Key")
            string minmax = XPath_Part.eFunction.Max == part.Function ? "Max" : "Min";
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
                value = XPath_Bracket.AttributeValue(path ?? maxPe, part, key);
            else
                value = XPath_Bracket.ElementValue(path ?? maxPe, part, key);
            
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

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
    public class Count
    {
        internal static Expression Nodes(Expression nodes)
        {
            Expression count = Expression.Call(
                    typeof(Enumerable),
                    "Count",
                    new[] { typeof(XElement) },
                    nodes
                    );
            return count;
        }
    }
}

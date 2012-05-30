// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq.Expressions;

namespace XmlLib.nXPath.Functions
{
    public class StartsWith
    {
        internal static Expression Parse(XPath_Part part, Expression left, Expression right, Expression path)
        {
            if (!(part.Value is string))
            {
                left = left.ToStringExpression();
                right = right.ToStringExpression();
            }
            return Expression.Call(
                left,
                typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                right);
        }
    }
}

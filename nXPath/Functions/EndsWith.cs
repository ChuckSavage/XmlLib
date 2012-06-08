﻿// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq.Expressions;

namespace XmlLib.nXPath.Functions
{
    internal class EndsWith : FunctionBase
    {
        internal EndsWith(XPath_Part part) : base(part) { }
        /// <summary>
        /// false
        /// </summary>
        internal override bool IsEqual { get { return false; } }

        internal override Expression Left(XPath_Part part, Expression left, Expression right, Expression path)
        {
            if (!(part.Value is string))
            {
                left = left.ToStringExpression();
                right = right.ToStringExpression();
            }
            left = Expression.Call(
                left,
                typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                right);
            return left;
        }
    }
}
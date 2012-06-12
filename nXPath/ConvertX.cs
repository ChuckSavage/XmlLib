// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq.Expressions;

namespace XmlLib.nXPath
{
    internal static class ConvertX<FROM, TO>
    {
        static Func<FROM, TO> convert;

        static ConvertX()
        {
            ParameterExpression pe = Expression.Parameter(typeof(FROM), "xe");
            Expression convertExpression = Expression.Convert(pe, typeof(TO));
            Expression<Func<FROM, TO>> lambda =
                Expression.Lambda<Func<FROM, TO>>(convertExpression, new ParameterExpression[] { pe });
            convert = lambda.Compile();
        }

        public static TO ToValue(FROM value)
        {
            return convert(value);
        }
    }
}
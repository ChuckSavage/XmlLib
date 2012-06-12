// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq.Expressions;

namespace XmlLib.nXPath
{
    internal static class Compare<T>
    {
        static Func<T, T, bool> equal;
        static Func<T, T, bool> lessThan;
        static Func<T, T, bool> greaterThan;

        static Func<T, T, bool> Compile(BinaryExpression e, ParameterExpression a, ParameterExpression b)
        {
            Expression<Func<T, T, bool>> lambda =
                Expression.Lambda<Func<T, T, bool>>(e, new ParameterExpression[] { a, b });
            return lambda.Compile();
        }

        static Compare()
        {
            ParameterExpression a = Expression.Parameter(typeof(T), "a");
            ParameterExpression b = Expression.Parameter(typeof(T), "b");
            Expression left = a;
            Expression right = b;
            if (typeof(string) == typeof(T))
            {
                // use string.Compare() 
                left = Expression.Call(
                    typeof(string),
                    "Compare",
                    null,
                    new Expression[] { left, right });
                right = Expression.Constant(0, typeof(int));
            }
            equal = Compile(Expression.Equal(left, right), a, b);
            lessThan = Compile(Expression.LessThan(left, right), a, b);
            greaterThan = Compile(Expression.GreaterThan(left, right), a, b);
        }

        public static bool Equal(T a, T b)
        {
            return equal(a, b);
        }

        public static bool GreaterThan(T a, T b)
        {
            return greaterThan(a, b);
        }

        public static bool GreaterThanOrEqual(T a, T b)
        {
            return !lessThan(a, b);
        }

        public static bool LessThan(T a, T b)
        {
            return lessThan(a, b);
        }

        public static bool LessThanOrEqual(T a, T b)
        {
            return !greaterThan(a, b);
        }

        public static bool NotEqual(T a, T b) { return !equal(a, b); }
    }
}
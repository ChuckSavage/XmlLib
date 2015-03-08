// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace XmlLib.nXPath
{
    internal static class XPathUtils
    {
        /// <summary>
        /// Get the Attribute from the parent node.
        /// </summary>
        internal static Expression Attribute(Expression parent, string key)
        {
            Expression att = Expression.Call(
                typeof(XElementExtensions),
                "GetAttribute",
                null,
                parent,
                Expression.Constant(key)
                );
            return att;
        }

        /// <summary>
        /// Get the Attribute value from the parent node.
        /// </summary>
        internal static Expression AttributeValue(Expression parent, XPath_Part part, string key)
        {
            Expression att = Attribute(parent, key);
            Expression value = Expression.Convert(att, part.Value.GetType());
            /*
            Expression isNull = Expression.Equal(att, Expression.Constant(null));
            Expression safe = Expression.Condition(isNull, GetDefault(part), att);
            Expression value = Expression.Convert(safe, part.Value.GetType());
             */
            return value;
        }

        internal static bool ContainsAny(this string source, params string[] values)
        {
            return values.Any(v => source.Contains(v));
        }

        internal static bool EndsWithAny(this string source, params string[] values)
        {
            return values.Any(v => source.EndsWith(v));
        }

        internal static bool TryEndsWithAny(this string source, out string with, params string[] values)
        {
            foreach (var v in values)
                if (source.EndsWith(v))
                {
                    with = v;
                    return true;
                }
            with = string.Empty;
            return false;
        }

        internal static bool StartsWithAny(this string source, params string[] values)
        {
            return values.Any(v => source.StartsWith(v));
        }

        /// <summary>
        /// Get the child element based on the key.
        /// </summary>
        internal static Expression Element(Expression parent, string key)
        {
            return Element(parent, ToXName(parent, key));
        }

        /// <summary>
        /// Get the child element based on the name.
        /// </summary>
        internal static Expression Element(Expression parent, Expression xname)
        {
            Expression ex = Expression.Call(
                parent,
                typeof(XElement).GetMethod("Element"),
                xname
                );
            return ex;
        }

        /// <summary>
        /// Get the child Element value from the parent node.
        /// </summary>
        internal static Expression ElementValue(Expression parent, XPath_Part part, string key)
        {
            Expression e = GetElement(parent, key);
            return Expression.Convert(e, part.Value.GetType());
            /*
            Expression value = Expression.Property(e, "Value");
            Expression isNullOrEmpty = Expression.Call(
                typeof(string),
                "IsNullOrEmpty",
                null,
                value);
            Func<Expression, Expression> Convert = exp => Expression.Convert(exp, part.Value.GetType());
            Expression safe = Expression.Condition(isNullOrEmpty, Convert(GetDefault(part)), Convert(e));
            return safe;
             */
        }
        /*
        /// <summary>
        /// Get a default expression based on the value type that is being evaluated in the part.
        /// </summary>
        internal static Expression GetDefault(XPath_Part part)
        {
            Type type = part.Value.GetType();
            object value = null;
            if (null != part.Function && part.Function.IsEqual)
            {
                value = part.Value;
            }
            if (null == value)
                try { value = Activator.CreateInstance(type); }
                catch (MissingMemberException)
                {
                    // what to do if type doesn't have a parameterless constructor?
                    // string has no string()
                    value = Guid.NewGuid();
                }
            if (part.IsValueAttribute || part.Key.Contains('@'))
                return Expression.Constant(new XAttribute("default", value));
            else
                return Expression.Constant(new XElement("default", value));
        }
        */

        /// <summary>
        /// Get the child element based on the key.
        /// Returns an empty element if not found.
        /// </summary>
        internal static Expression GetElement(Expression parent, string key)
        {
            Expression ex = Expression.Call(
                                typeof(XElementExtensions),
                                "GetElement",
                                null,
                                parent,
                                Expression.Constant(key)
                                );
            return ex;
        }

        /// <summary>
        /// var getRowValExpr = GetMethodExpression(x => x.GetRowValue(0));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static MethodCallExpression GetMethodExpression<T>(Expression<Func<T>> e)
        {
            return e.Body as MethodCallExpression;
        }

        internal static Expression NullCheckReturnsBool(this Expression e, Expression aok)
        {
            /* if(null == expression)
             *     return false;
             * return aok; // which should evaluate to a true/false condition
             */
            Expression isNull = Expression.Equal(e, Expression.Constant(null));
            Expression @false = Expression.Constant(false);
            Expression safe = Expression.Condition(isNull, @false, aok);
            return safe;
        }

        /// <summary>
        /// Call .ToString() on the Expression value.
        /// </summary>
        internal static Expression ToStringExpression(this Expression source)
        {
            return Expression.Call(source, typeof(string).GetMethod("ToString", Type.EmptyTypes));
        }

        /// <summary>
        /// Convert Attribute or Element key to the proper XName via the parent XElement node.
        /// </summary>
        internal static Expression ToXName(Expression parent, string key)
        {
            Expression toXName = Expression.Call(
                typeof(XElementExtensions),
                "ToXName",
                null,
                parent,
                Expression.Constant(key)
                );
            return toXName;
        }
    }
}
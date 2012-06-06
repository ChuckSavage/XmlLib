// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using XmlLib.nXPath.Functions;

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{XPath}")]
    internal class XPath_Bracket
    {
        public delegate Expression DCompareAttribute(XPath_Part part, string key, Expression left, Expression right);
        public delegate Expression DCompareElement(XPath_Part part, string key, Expression parent, Expression left, Expression right);

        internal readonly static ParameterExpression pe = Expression.Parameter(typeof(XElement), "xe");
        internal readonly static ParameterExpression pa = Expression.Parameter(typeof(XAttribute), "xa");

        public readonly XPath_Part[] Parts;
        public readonly bool AndOr;

        /// <summary>
        /// The expression.
        /// </summary>
        public string XPath { get; private set; }

        public XPath_Bracket(XPathString path)
        {
            string text = path.Format;
            XPath = path.Text.TrimStart('[').TrimEnd(']');
            text = text.TrimStart('[').TrimEnd(']');
            AndOr = text.Contains(" and ");

            // need new XPathString() that has brackets removed.
            XPathString[] paths = new XPathString(true, text, path.Values)
                .Split(new string[] { " and ", " or " }, StringSplitOptions.RemoveEmptyEntries);
            Parts = paths.Select(s => new XPath_Part(s)).ToArray();
        }

        internal static Expression ExpressionEquals(XPath_Part part, Expression left, Expression right)
        {
            return ExpressionEquals(part, left, right, null);
        }

        internal static Expression ExpressionEquals(XPath_Part part, Expression left, Expression right, Expression path)
        {
            Expression ex;
            if (null != part.Function)
            {
                left = part.Function.Left(part, left, right, path);
                right = part.Function.Right(part, left, right, path);
                if (!part.Function.IsEqual)
                    return left;
            }
            if (part.Value is string)
            {
                // use string.Compare() 
                left = Expression.Call(
                    typeof(string),
                    "Compare",
                    null,
                    new Expression[] { left, right });
                right = Expression.Constant(0, typeof(int));
            }

            if (part.NotEqual)
                ex = Expression.NotEqual(left, right);
            else if (part.LessThanOrEqual)
                ex = Expression.LessThanOrEqual(left, right);
            else if (part.GreaterThanOrEqual)
                ex = Expression.GreaterThanOrEqual(left, right);
            else if (part.Equal)
                ex = Expression.Equal(left, right);
            else if (part.LessThan)
                ex = Expression.LessThan(left, right);
            else if (part.GreaterThan)
                ex = Expression.GreaterThan(left, right);
            else
                throw new NotImplementedException(part.self.Text);
            return ex;
        }

        protected Expression CompareAttribute(XPath_Part part, string key, Expression _elements, Expression right)
        {
            if ("*" != key)
            {
                Expression att = XPathUtils.AttributeValue(_elements ?? pe, part, key);
                return ExpressionEquals(part, att, right);
            }
            else // [@*='ABC']
            {
                Expression attributes = Expression.Call(
                    _elements ?? pe,
                    "Attributes",
                    null);
                Expression type = Expression.Convert(pa, part.Value.GetType());
                Expression equal = ExpressionEquals(part, type, Expression.Constant(part.Value));
                return Expression.Call(
                        typeof(Enumerable),
                        "Any",
                        new[] { typeof(XAttribute) },
                        attributes,
                        Expression.Lambda<Func<XAttribute, bool>>(equal, new ParameterExpression[] { pa })
                       );
            }
        }

        public IEnumerable<XElement> Elements(IEnumerable<XElement> elements)
        {
            Expression _elements = null;
            IQueryable<XElement> query = elements.AsQueryable<XElement>();
            MethodCallExpression call;
            Expression right = null, ex = null, e;
            string method = "Where";

            foreach (XPath_Part part in Parts)
            {
                e = null;
                right = Expression.Constant(part.Value);
                if (part.ElementAt)
                {
                    method = "ElementAt";
                    int i = Convert.ToInt32(part.Value);
                    switch (part.Key)
                    {
                        case "last()":
                            ex = Count.Nodes(query.Expression);
                            // last() - 1 is the last element so it should be last() - (1 + 1)
                            ex = Expression.Subtract(ex, Expression.Constant(i + 1));
                            break;
                        default:
                            // [1] should be 0
                            ex = Expression.Constant(i - 1);
                            break;
                    }
                    break; // break out of foreach
                }
                else if (null == part.Value)
                {
                    e = HasChildNodes(part);
                }
                else
                {
                    // Parse Key Value Pair Expression
                    string[] split = part.Key.TrimStart('/').Split('/');
                    _elements = null;
                    for (int i = 0; i < split.Length; i++)
                    {
                        string key = split[i];
                        bool last = (i + 1) == split.Length;
                        if (part.IsValueAttribute || key.StartsWith("@"))
                        {
                            key = key.TrimStart('@');
                            if (null != part.Function && null != part.Function.CompareAttribute)
                                e = part.Function.CompareAttribute(part, key, _elements ?? pe, right);
                            else
                                e = CompareAttribute(part, key, _elements, right);
                            break;
                        }
                        else
                        {
                            Expression parent = _elements;
                            switch (key)
                            {
                                case null:
                                case "":
                                case "*": // [*='ABC']
                                    _elements = Expression.Call(
                                        _elements ?? pe,
                                        typeof(XElement).GetMethod("Elements", Type.EmptyTypes)
                                        );
                                    break;
                                case ".": // [.='ABC'] means current node equals
                                    _elements = _elements ?? pe;
                                    break;
                                default:
                                    _elements = Expression.Call(
                                        typeof(XElementExtensions),
                                        last ? "GetElements" : "GetElement",
                                        null,
                                        _elements ?? pe,
                                        Expression.Constant(key)
                                        );
                                    break;
                            }
                            if (last)
                            {
                                if (null != part.Function && null != part.Function.CompareElement)
                                    e = part.Function.CompareElement(part, key, parent ?? pe, _elements ?? pe, right);
                                else
                                {
                                    Expression type = Expression.Convert(pe, part.Value.GetType());
                                    Expression equal = ExpressionEquals(part, type, Expression.Constant(part.Value));
                                    if ("." == key) // [.='ABC'] means current node equals
                                        e = equal;
                                    else
                                        e = Expression.Call(
                                            typeof(Enumerable),
                                            "Any",
                                            new[] { typeof(XElement) },
                                            _elements,
                                            Expression.Lambda<Func<XElement, bool>>(equal, new ParameterExpression[] { pe })
                                           );
                                }
                            }
                        }
                    }
                }
                if (null == ex)
                    ex = e;
                else if (AndOr)
                    ex = Expression.AndAlso(ex, e);
                else
                    ex = Expression.OrElse(ex, e);
            }

            // if method returns a single element
            if ("ElementAt" == method)
            {
                call = Expression.Call(
                        typeof(Queryable),
                        method, // "ElementAt"
                        new Type[] { query.ElementType },
                        query.Expression,
                        ex
                    );
                XElement result = query.Provider.Execute<XElement>(call);
                return new[] { result };
            }
            else // if method returns an enumerable
            {
                call = Expression.Call(
                       typeof(Queryable),
                       method, // "Where" 
                       new Type[] { query.ElementType },
                       query.Expression,
                       Expression.Lambda<Func<XElement, bool>>(ex, new ParameterExpression[] { pe })
                       );
                IQueryable<XElement> results = query.Provider.CreateQuery<XElement>(call);
                return results;
            }
        }

        private Expression HasChildNodes(XPath_Part part)
        {
            Expression _elements = null;
            string path = part.Key, att = string.Empty;
            bool attrib = false, star = false;

            if (attrib = part.IsValueAttribute)
            {
                if (path.Contains('/'))
                {
                    string[] parts = path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    att = parts.Last();
                    path = string.Join("/", parts.Take(parts.Length - 1).ToArray());
                }
                else
                {
                    att = path;
                    path = string.Empty;
                }
            }
            else if (attrib = path.Contains('@'))
            {
                string[] parts = path.Split('@');
                path = string.Join("/", parts[0].Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray());
                att = parts[1];
            }
            if (star = path.Contains('*'))
            {
                throw new NotImplementedException();
            }
            if (!string.IsNullOrEmpty(path))
                _elements = Expression.Call(
                            typeof(XElementExtensions),
                            attrib ? "GetElement" : "GetElements",
                            null,
                            pe,
                            Expression.Constant(path)
                            );
            if (attrib)
            {
                Expression a = XPathUtils.Attribute(_elements ?? pe, att);
                return Expression.NotEqual(
                    Expression.Constant(null, typeof(XAttribute)),
                    a
                    );
            }
            else
            {
                // Handles [nodes] nodes being greater than zero
                return Expression.GreaterThan(
                    Count.Nodes(_elements ?? pe),
                    Expression.Constant(0)
                    );
            }
        }
    }
}
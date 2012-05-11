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
#if SeaRisenLib2
using SeaRisenLib2.Xml;
#endif

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{XPath}")]
    internal class XPath_Bracket
    {
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
            XPathString[] paths = new XPathString(text, path.Values)
                .Split(new string[] { " and ", " or " }, StringSplitOptions.RemoveEmptyEntries);
            Parts = paths.Select(s => new XPath_Part(s)).ToArray();
            var p = Parts;
        }

        private Expression ExpressionEquals(XPath_Part part, Expression left, Expression right)
        {
            Expression ex;
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

        public IEnumerable<XElement> Elements(XElement contextNode, IEnumerable<XElement> elements)
        {
            IQueryable<XElement> query = elements.AsQueryable<XElement>();
            MethodCallExpression call;
            Expression left, right, ex = null, e;
            string method = "Where";
            ParameterExpression pe = Expression.Parameter(typeof(XElement), "XElement");
            ParameterExpression pa = Expression.Parameter(typeof(XAttribute), "XAttribute");

            foreach (XPath_Part part in Parts)
            {
                e = null;
                if (part.ElementAt)
                {
                    method = "ElementAt";
                    int i = Convert.ToInt32(part.Value);
                    switch (part.Key)
                    {
                        case "last()":
                            ex = Expression.Call(
                                typeof(Queryable),
                                "Count",
                                new[] { query.ElementType },
                                query.Expression);
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

                // Parse Key Value Pair Expression
                right = Expression.Constant(part.Value);
                string[] split = part.Key.TrimStart('/').Split('/');
                Expression _elements = null;
                for (int i = 0; i < split.Length; i++)
                {
                    string key = split[i];
                    bool last = (i + 1) == split.Length;
                    if (part.IsValueAttribute || key.StartsWith("@"))
                    {
                        key = key.TrimStart('@');
                        
                        // How to get value of _elements into variable to next two calls?

                        Expression toXName = Expression.Call(
                            typeof(XElementExtensions),
                            "ToXName",
                            null,
                            null == _elements ? pe : _elements,//new [] { typeof(string) },
                            Expression.Constant(key)
                            );
                        
                        Expression att = Expression.Call(
                            null == _elements ? pe : _elements,
                            typeof(XElement).GetMethod("Attribute", new Type[] { typeof(XName) }),
                            toXName
                            );
                        att = Expression.Convert(att, part.Value.GetType());
                        e = ExpressionEquals(part, att, right);

                        break;
                    }
                    else
                    {
                        _elements = Expression.Call(
                        typeof(XElementExtensions),
                        last ? "GetElements" : "GetElement",
                        null,
                        null == _elements ? pe : _elements,
                        Expression.Constant(key)
                        );
                        if (last)
                        {
                            if (null != part.Value)
                            {
                                Expression type = Expression.Convert(pe, part.Value.GetType());
                                Expression equal = ExpressionEquals(part, type, Expression.Constant(part.Value));

                                e = Expression.Call(
                                    typeof(Enumerable),
                                    "Any",
                                    new[] { typeof(XElement) },
                                    _elements,
                                    Expression.Lambda<Func<XElement, bool>>(equal, new ParameterExpression[] { pe })
                                   );
                            }
                            else
                            {
                                Expression count = Expression.Call(
                                    typeof(Enumerable),
                                    "Count",
                                    new[] { typeof(XElement) },
                                    _elements
                                    );

                                e = Expression.GreaterThan(
                                    count,
                                    Expression.Constant(0)
                                    );
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
    }
}
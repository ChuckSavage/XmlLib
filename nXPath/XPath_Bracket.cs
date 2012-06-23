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

        public IEnumerable<XElement> Elements(IEnumerable<XElement> elements)
        {
            IQueryable<XElement> query = elements.AsQueryable<XElement>();
            Expression ex = null, e;
            string method = "Where";

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
                FunctionBase f = part.Function;
                if (null == f)
                {
                    if (part.KVP)
                        f = new KVP(part);
                    else
                        f = new HasNode(part); // [nodeset]
                }
                e = f.GetExpression(pe);
                if (null == ex)
                    ex = e;
                else if (AndOr)
                    ex = Expression.AndAlso(ex, e);
                else
                    ex = Expression.OrElse(ex, e);
            }

            MethodCallExpression call;
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
            bool attrib = false, star;

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
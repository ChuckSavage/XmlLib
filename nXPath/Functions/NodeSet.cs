// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    internal class NodeSet : FunctionBase
    {
        internal NodeSet(XPath_Part part) : base(part) { }

        internal override Expression GetExpression(Expression node)
        {
            throw new NotImplementedException();
        }

        static XElement GetElement(XElement source, string key)
        {
            XElement temp = source.Element(key);
            if (null == temp)
            {
                XName xname = source.ToXName(key);
                temp = source.Element(xname);
            }
            return temp;
        }

        static IEnumerable<XElement> GetElements(XElement source, string key)
        {
            if ("*" == key)
                return source.Elements();
            var temp = source.Elements(key);
            if (0 == temp.Count())
            {
                XName xname = source.ToXName(key);
                temp = source.Elements(xname);
            }
            return temp.ToArray();
        }

        internal object Node(XElement x, string nodeset)
        {
            string key;
            return Node(x, nodeset, out key);
        }

        internal object Node(XElement x, string nodeset, out string key)
        {
            key = nodeset;
            if ("." == nodeset) return x;
            if (".." == nodeset) return x.Parent;
            if (nodeset.Contains('/'))
            {
                string[] split = nodeset.Split('/');
                x = GetElement(x, split[0]);
                if (null == x) return null;
                return Node(x, nodeset.Substring(split[0].Length + 1), out key);
            }
            if (part.IsValueAttribute || nodeset.Contains('@'))
            {
                if (nodeset.Contains('@'))
                    nodeset = nodeset.Split('@').Last();
                key = nodeset;
                if ("*" == nodeset)
                    return x.Attributes().ToArray();
                return x.GetAttribute(nodeset);
            }
            return GetElements(x, nodeset);
        }

        T Convert<T>(object result, string key)
        {
            T value = default(T);
            XElement node = null;
            try
            {
                if (result is XAttribute)
                {
                    XAttribute a = (XAttribute)result;
                    node = a.Parent;
                    value = ConvertX<XAttribute, T>.ToValue(a);
                }
                else if (result is XElement)
                    value = ConvertX<XElement, T>.ToValue(node = result as XElement);
            }
            catch (FormatException)
            {
                T @default = default(T);
                if (part.Function is MinMax)
                    @default = (T)part.Value;
                if (null != node)
                    value = node.Get(key, @default);
            }
            return value;
        }

        internal bool NodeValue<T>(XElement node, out T[] values)
        {
            return NodeValue<T>(node, Convert<T>, out values);
        }

        internal bool NodeValue<T>(XElement node, Func<object, string, T> convert, out T[] values)
        {
            values = new[] { default(T) };
            string key;
            object result = Node(node, part.Key, out key);
            if (null != result)
            {
                IEnumerable<T> list;
                IEnumerable<XElement> nodes = result as IEnumerable<XElement>;
                if (null != nodes)
                    list = nodes.Select(x => convert(x, key));
                else
                {
                    IEnumerable<XAttribute> atts = result as IEnumerable<XAttribute>;
                    if (null != atts)
                        list = atts.Select(a => convert(a, key));
                    else
                        list = new[] { convert(result, key) };
                }
                values = list.ToArray();
                return true;
            }
            return false;
        }

        #region old
        static string ParseInternal(XPath_Part part, string path,
            out string sAttribute,
            out bool bAttribute
            )
        {
            sAttribute = string.Empty;
            bAttribute = false;
            bool star = false;

            if (bAttribute = part.IsValueAttribute)
            {
                if (path.Contains('/'))
                {
                    string[] parts = path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    sAttribute = parts.Last();
                    path = string.Join("/", parts.Take(parts.Length - 1).ToArray());
                }
                else
                {
                    sAttribute = path;
                    path = string.Empty;
                }
            }
            else if (bAttribute = path.Contains('@'))
            {
                string[] parts = path.Split('@');
                path = string.Join("/", parts[0].Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray());
                sAttribute = parts[1];
            }
            if (star = path.Contains('*'))
            {
                throw new NotImplementedException();
            }
            return path;
        }

        internal static Expression Parse(XPath_Part part, string path, bool enumerableResult)
        {
            Expression elements = null;
            string sAttribute;
            bool bAttribute;
            path = ParseInternal(part, path, out sAttribute, out bAttribute);
            if (!string.IsNullOrEmpty(path))
                elements = Expression.Call(
                            typeof(XElementExtensions),
                            bAttribute || !enumerableResult ? "GetElement" : "GetElements",
                            null,
                            pe,
                            Expression.Constant(path)
                            );
            if (bAttribute)
                elements = XPathUtils.Attribute(elements ?? pe, sAttribute);
            else if (null == elements)
                throw new NotImplementedException("Empty path");
            return elements;
        }

        /// <summary>
        /// Bool check each level of path, if any don't exist, the whole thing is false.
        /// </summary>
        /// <returns>An Expression that is either true or the expression to evaluate to either true or false</returns>
        internal static Expression GetElementsSafe(XPath_Part part, string path)
        {
            Expression elements = pe;
            string sAttribute;
            bool bAttribute;
            path = ParseInternal(part, path, out sAttribute, out bAttribute);
            {
                //Expression @null = Expression.Constant(null, typeof(XElement));
                //string[] split = path.Split('/');
                //string sLast = split.Last();
                if (!string.IsNullOrEmpty(path))
                    elements = Expression.Call(
                            typeof(XElementExtensions),
                            "GetElements",
                            null,
                            pe,
                            Expression.Constant(path)
                            );
                else
                    elements = Expression.Call(
                        pe,
                        typeof(XElement).GetMethod("Elements"),
                        null,
                        Expression.Constant(Type.EmptyTypes)
                        );

                Expression notNull;
                if (bAttribute)
                {
                    notNull = Expression.NotEqual(XPathUtils.Attribute(pe, sAttribute), Expression.Constant(null));
                }
                else
                {
                    notNull = Expression.NotEqual(pe, Expression.Constant(null));
                }

                Expression where = Expression.Call(
                        typeof(Enumerable),
                        "Where",
                        new Type[] { typeof(XElement) },
                        elements,
                        Expression.Lambda<Func<XElement, bool>>(notNull, new ParameterExpression[] { XPath_Bracket.pe })
                        );
                elements = where;
            }
            return elements;
        }

        /// <summary>
        /// Bool check each level of path, if any don't exist, the whole thing is false.
        /// </summary>
        /// <returns>An Expression that the path exists or not</returns>
        internal static Expression PathExists(XPath_Part part, string path)
        {
            Expression element = pe;
            Expression result = Expression.Constant(false);
            string sAttribute;
            bool bAttribute;
            path = ParseInternal(part, path, out sAttribute, out bAttribute);
            if (!string.IsNullOrEmpty(path))
            {
                Expression @null = Expression.Constant(null, typeof(XElement));
                string[] split = path.Split('/');
                string sLast = split.Last();
                foreach (string name in split)
                {
                    Expression parent = element;
                    element = Expression.Call(
                        element,
                        typeof(XElement).GetMethod("Element"),
                        XPathUtils.ToXName(element, name)
                        );
                    Expression equal = Expression.Equal(@null, element);
                    result = Expression.Condition(equal, result, equal);
                }
            }
            return result;
        }
        #endregion
    }
}
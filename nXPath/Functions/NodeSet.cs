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
            if (source.HasElements)
            {
                XElement temp = source.Element(key);
                if (null == temp)
                {
                    XName xname = source.ToXName(key);
                    temp = source.Element(xname);
                }
                return temp;
            }
            return null;
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
                if (null != node)
                {
                    // check for attribute or child node existing before calling .Get()
                    if (null != node.GetAttribute(key) || null != GetElement(node, key))
                    {
                        T @default = default(T);
                        if (part.Function is MinMax)
                            @default = (T)part.Value;
                        // Get will try and create the node if not exists
                        value = node.Get(key, @default);
                    }
                }
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
    }
}
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

        static NodeResult GetElements(XElement source, string key)
        {
            if ("*" == key)
                return new NodeResult(source.Elements().ToArray(), NodeResult.eResultType.ElementArray);
            var temp = source.Elements(key);
            if (0 == temp.Count())
            {
                XName xname = source.ToXName(key);
                temp = source.Elements(xname);
            }
            return new NodeResult(temp.ToArray(), NodeResult.eResultType.ElementArray);
        }

        internal NodeResult Node(XElement x, string nodeset)
        {
            string key;
            return Node(x, nodeset, out key);
        }

        internal NodeResult Node(XElement x, string nodeset, out string key)
        {
            key = nodeset.Trim();
            if ("." == nodeset)
                return new NodeResult(x, NodeResult.eResultType.Element);
            if (".." == nodeset) return new NodeResult(x.Parent, NodeResult.eResultType.Element); ;
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
                    return new NodeResult(x.Attributes().ToArray(), NodeResult.eResultType.AttributeArray);
                XAttribute a = x.GetAttribute(nodeset);
                if (null == a)
                    return null;
                return new NodeResult(a, NodeResult.eResultType.Attribute);
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
            // if nodeset is current node and it has children, it is false
            if ("." == part.Key && node.HasElements)
                return false;

            string key;
            NodeResult result = Node(node, part.Key, out key);
            if (null != result)
            {
                IEnumerable<T> list = values;
                switch (result.ResultType)
                {
                    case NodeResult.eResultType.ElementArray:
                        list = result.ElementArray.Select(x => convert(x, key));
                        break;
                    case NodeResult.eResultType.AttributeArray:
                        list = result.AttributeArray.Select(x => convert(x, key));
                        break;
                    case NodeResult.eResultType.Attribute:
                    case NodeResult.eResultType.Element:
                        list = new[] { convert(result.Result, key) };
                        break;
                }
                values = list.ToArray();
                return true;
            }
            return false;
        }
 
    }
}
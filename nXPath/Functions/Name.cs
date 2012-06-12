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
    /// <summary>
    /// <para>Example: root2.XPathElement("//*[name()={0}]", "div");</para>
    /// <para>Syntax: name()</para>
    /// <para>Syntax: name(nodeset)</para>
    /// <para>- nodeset: path to a node or attribute, and its name.</para>
    /// <para>Returns the name of the current node or the first node in the specified node set</para>
    /// </summary>
    internal class Name : FunctionBase
    {
        public Name(XPath_Part part)
            : base(part, typeof(NameGeneric<>))
        {
            //_CompareAttribute = Name_CompareAttribute;
            //_CompareElement = Name_CompareElement;
        }

        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsRequired { get { return false; } }
        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsValueRequired { get { return false; } }

        internal class NameGeneric<T> : GenericBase
        {
            Name self;

            public NameGeneric(Name name, XElement nodeToCheck)
                : base(nodeToCheck, name.part)
            {
                self = name;
            }

            bool IsEqual(object node)
            {
                XName xname = Name(node);
                if (null != xname)
                {
                    bool result = xname == ((XName)part.Value);
                    return result;
                }
                return false;
            }

            XName Name(object node)
            {
                if (node is XAttribute)
                    return ((XAttribute)node).Name;
                if (node is XElement)
                    return ((XElement)node).Name;
                return null;
            }

            public override bool Eval()
            {
                try
                {
                    if (string.IsNullOrEmpty(part.Key)) return IsEqual(node);
                    object result = nodeset.Node(node, part.Key);
                    if (null == result) return false;
                    IEnumerable<object> list = result as IEnumerable<object>;
                    if (null == list)
                        return IsEqual(result);
                    return list.Any(x => IsEqual(x));
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                return false;
            }

            public override void Init()
            {
                try
                {
                }
                catch (Exception ex) { error = ex; }
            }
        }

        #region old
        bool byAttribute = false;

        internal Expression Name_CompareAttribute(XPath_Part part, string key, Expression left, Expression right)
        {
            byAttribute = true;
            if ("*" != key)
            {
                Expression att = XPathUtils.Attribute(left, key);
                Expression isNull = Expression.Equal(att, Expression.Constant(null));
                Expression name = Expression.Property(att, "Name");
                Expression equals = XPath_Bracket.ExpressionEquals(part, name, right);
                Expression safe = Expression.Condition(isNull, Expression.Constant(false), equals);
                return safe;
            }
            else // [name(@*)={0}], ns + "id"
            {
                // x => x.Attributes().Any(xa => xa.Name == ns + "id")
                Expression attributes = Expression.Call(
                    left,
                    "Attributes",
                    null);
                Expression name = Expression.Property(XPath_Bracket.pa, "Name");
                Expression equal = XPath_Bracket.ExpressionEquals(part, name, Expression.Constant(part.Value));
                Expression any = Expression.Call(
                        typeof(Enumerable),
                        "Any",
                        new[] { typeof(XAttribute) },
                        attributes,
                        Expression.Lambda<Func<XAttribute, bool>>
                            (equal, new ParameterExpression[] { XPath_Bracket.pa })
                       );
                return any;
            }
        }

        internal Expression Name_CompareElement(XPath_Part part, string key, Expression parent, Expression left, Expression right)
        {
            // x => x.Elements().Any(xx => xx.Name == ns + "div")
            Expression e;
            Expression equal = XPath_Bracket.ExpressionEquals(part, null, Expression.Constant(part.Value));
            if ("." == key) // [name(.)={0}], ns + tagName or (XName)"tagName"
                e = equal;
            else
                e = Expression.Call(
                    typeof(Enumerable),
                    "Any",
                    new[] { typeof(XElement) },
                    left,
                    Expression.Lambda<Func<XElement, bool>>(equal, new ParameterExpression[] { XPath_Bracket.pe })
                   );
            return e;
        }

        internal override Expression Left(XPath_Part part, Expression left, Expression right, Expression path)
        {
            if (byAttribute)
                return left; // we already computed it
            return Expression.Property(XPath_Bracket.pe, "Name");
        }
        #endregion
    }
}
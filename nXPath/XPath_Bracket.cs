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

namespace XmlLib.nXPath
{
    [DebuggerDisplay("{XPath}")]
    internal class XPath_Bracket
    {
        ParameterExpression pe = Expression.Parameter(typeof(XElement), "xe");
        ParameterExpression pa = Expression.Parameter(typeof(XAttribute), "xa");

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
        }

        private Expression ExpressionEquals(XPath_Part part, Expression left, Expression right)
        {
            return ExpressionEquals(part, left, right, null);
        }

        private Expression ExpressionEquals(XPath_Part part, Expression left, Expression right, Expression path)
        {
            Expression ex;
            if (part.Value is string)
            {
                if (XPath_Part.eFunction.StartsWith == part.Function)
                {
                    if (!(part.Value is string))
                    {
                        Func<Expression, Expression> Expression_ToString = e =>
                            Expression.Call(e, typeof(string).GetMethod("ToString", Type.EmptyTypes));
                        left = Expression_ToString(left);
                        right = Expression_ToString(right);
                    }
                    return Expression.Call(
                        left,
                        typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                        right);
                }
                else
                {
                    // use string.Compare() 
                    left = Expression.Call(
                        typeof(string),
                        "Compare",
                        null,
                        new Expression[] { left, right });
                    right = Expression.Constant(0, typeof(int));
                }
            }
            else if (XPath_Part.eFunction.None != part.Function)
            {
                switch (part.Function)
                {
                    /*
                     * Min/Max get the xelement's parent's elements and find the min/max value
                     */
                    case XPath_Part.eFunction.Max:
                    case XPath_Part.eFunction.Min:
                        // right = x.Parent.Elements(x.Name).Max(xx => (int)xx.Attribute("Key")
                        string max = XPath_Part.eFunction.Max == part.Function ? "Max" : "Min";
                        string[] keyParts = part.Key.Split('/');
                        string key = keyParts.Last();
                        ParameterExpression maxPe = Expression.Parameter(typeof(XElement), max.ToLower());
                        Expression parent = Expression.Property(pe, "Parent");
                        Expression value;
                        if (keyParts.Length > 1)
                            path = Expression.Call(
                                    typeof(XElementExtensions),
                                    "GetElement",
                                    null,
                                    maxPe,
                                    Expression.Constant(string.Join("/", keyParts.Take(keyParts.Length - 1).ToArray()))
                                    );

                        if (part.IsValueAttribute)
                            value = AttributeValue(path ?? maxPe, part, key);
                        else
                        {
                            value = ElementValue(path ?? maxPe, part, key);
                        }
                        Expression name = Expression.Property(pe, "Name");
                        Expression elements = Expression.Call(
                                typeof(XElementExtensions),
                                "GetElements",
                                null,
                                parent,
                                name
                                );
                        right = Expression.Call(
                                typeof(Enumerable),
                                max,
                                new[] { typeof(XElement), part.Value.GetType() },
                                elements,
                                Expression.Lambda(value, new ParameterExpression[] { maxPe }));
                        break;
                }
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

        protected Expression Attribute(Expression parent, string key)
        {
            Expression att = Expression.Call(
                parent,
                typeof(XElement).GetMethod("Attribute", new Type[] { typeof(XName) }),
                ToXName(parent, key)
                );
            return att;
        }

        protected Expression AttributeValue(Expression parent, XPath_Part part, string key)
        {
            Expression att = Attribute(parent, key);
            Expression isNull = Expression.Equal(att, Expression.Constant(null));
            Expression safe = Expression.Condition(isNull, GetDefault(part), att);
            Expression value = Expression.Convert(safe, part.Value.GetType());
            return value;
        }

        /// <summary>
        /// Returns an empty element if not found.
        /// </summary>
        protected Expression Element(Expression parent, string key)
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

        protected Expression ElementValue(Expression parent, XPath_Part part, string key)
        {
            Expression e = Element(parent, key);
            Expression value = Expression.Property(e, "Value");
            Expression isNullOrEmpty = Expression.Call(
                typeof(string),
                "IsNullOrEmpty",
                null,
                value);
            Func<Expression, Expression> Convert = exp => Expression.Convert(exp, part.Value.GetType());
            Expression safe = Expression.Condition(isNullOrEmpty, Convert(GetDefault(part)), Convert(e));
            return safe;
        }

        protected Expression CompareAttribute(XPath_Part part, string key, Expression _elements, Expression right)
        {
            if ("*" != key)
            {
                Expression att = AttributeValue(_elements ?? pe, part, key);
                return ExpressionEquals(part, att, right);
            }
            else // [@*='ABC']
            {
                Expression attributes = Expression.Call(
                    null == _elements ? pe : _elements,
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

        public IEnumerable<XElement> Elements(XElement contextNode, IEnumerable<XElement> elements)
        {
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
                string[] split = part.Key.TrimStart('/').Split('/');
                Expression _elements = null;
                for (int i = 0; i < split.Length; i++)
                {
                    string key = split[i];
                    bool last = (i + 1) == split.Length;
                    if (part.IsValueAttribute || key.StartsWith("@"))
                    {
                        key = key.TrimStart('@');
                        e = CompareAttribute(part, key, _elements, right);
                        break;
                    }
                    else
                    {
                        Expression parent = _elements;
                        if ("*" != key)
                        {
                            _elements = Expression.Call(
                                typeof(XElementExtensions),
                                last ? "GetElements" : "GetElement",
                                null,
                                null == _elements ? pe : _elements,
                                Expression.Constant(key)
                                );
                        }
                        else // [*='ABC']
                        {
                            _elements = Expression.Call(
                                null == _elements ? pe : _elements,
                                typeof(XElement).GetMethod("Elements", Type.EmptyTypes)
                                );
                        }
                        if (last)
                        {
                            switch (part.Function)
                            {
                                case XPath_Part.eFunction.Max:
                                case XPath_Part.eFunction.Min:
                                    Expression left = ElementValue(parent ?? pe, part, key);
                                    e = ExpressionEquals(part, left, right, null);
                                    break;
                                default:
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
                                    break;
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

        protected Expression GetDefault(XPath_Part part)
        {
            Type type = part.Value.GetType();
            object value = null;
            switch (part.Function)
            {
                case XPath_Part.eFunction.Max:
                case XPath_Part.eFunction.Min:
                    value = part.Value;
                    break;
            }
            if (null == value)
                try { value = Activator.CreateInstance(type); }
                catch (MissingMemberException) 
                {
                    return Expression.Constant(null);
                }
            if (part.IsValueAttribute || part.Key.Contains('@'))
                return Expression.Constant(new XAttribute("default", value));
            else
                return Expression.Constant(new XElement("default", value));
        }

        protected Expression ToXName(Expression parent, string key)
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
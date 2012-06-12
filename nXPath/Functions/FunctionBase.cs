﻿// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    internal class FunctionBase
    {
        protected XPath_Part part;
        Type generic;

        public FunctionBase(XPath_Part part, Type genericType)
        {
            this.part = part;
            generic = genericType;
        }

        public FunctionBase(XPath_Part part) : this(part, null) { }

        public virtual bool Internal(XElement nodeToCheck)
        {
            Type type = (part.Value ?? 0).GetType(); // 0 for generic types that have no type

            var gmm = generic.MakeGenericType(new[] { type });

            var instance = gmm
                .GetConstructor(new[] { this.GetType(), typeof(XElement) })
                .Invoke(new object[] { this, nodeToCheck })
                as GenericBase;

            try
            {
                instance.Init(); // gmm.GetMethod("Init").Invoke(instance, null);
                return instance.Eval(); // return (bool)gmm.GetMethod("Eval").Invoke(instance, null);
            }
            catch (TargetInvocationException)
            {
                FieldInfo info = instance.GetType().GetField("error");
                Exception e = info.GetValue(instance) as Exception;
                throw e;
            }
        }

        internal virtual Expression GetExpression(Expression node)
        {
            Expression @this = Expression.Constant(this);
            return Expression.Call(
                @this,
                typeof(MinMax).GetMethod("Internal"),
                node);
        }

        #region old
        protected static ParameterExpression pe = XPath_Bracket.pe;
        protected static ParameterExpression pa = XPath_Bracket.pa;
        /// <summary>
        /// Compare Attribute value Expression generator, if different than the norm.
        /// </summary>
        internal XPath_Bracket.DCompareAttribute CompareAttribute { get { return _CompareAttribute; } }
        protected XPath_Bracket.DCompareAttribute _CompareAttribute = null;

        /// <summary>
        /// Compare Element value Expression generator, if different than the norm.
        /// </summary>
        internal XPath_Bracket.DCompareElement CompareElement { get { return _CompareElement; } }
        protected XPath_Bracket.DCompareElement _CompareElement = null;

        /// <summary>
        /// true
        /// </summary>
        internal virtual bool HasKVP { get { return true; } }
        /// <summary>
        /// true
        /// </summary>
        internal virtual bool ArgumentsRequired { get { return true; } }
        /// <summary>
        /// true
        /// </summary>
        internal virtual bool ArgumentsValueRequired { get { return true; } }
        /// <summary>
        /// true
        /// </summary>
        internal virtual bool IsEqual { get { return true; } }

        internal virtual Expression Left(XPath_Part part, Expression left, Expression right, Expression path)
        {
            return left;
        }

        internal virtual Expression Right(XPath_Part part, Expression left, Expression right, Expression path)
        {
            return right;
        }
        #endregion
    }
}

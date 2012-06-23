// Copyright SeaRisen LLC
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

        /// <summary>
        /// Arguments to the function.
        /// <remarks>Set by XPath_Part</remarks>
        /// </summary>
        internal object[] args = { };

        public FunctionBase(XPath_Part part, Type genericType)
        {
            this.part = part;
            generic = genericType;
        }

        public FunctionBase(XPath_Part part) : this(part, null) { }

        protected GenericBase Generic;

        public virtual bool Internal(XElement nodeToCheck)
        {
            if (null == Generic)
            {
                Type type = (part.Value ?? 0).GetType(); // 0 for generic types that have no type

                var gmm = generic.MakeGenericType(new[] { type });

                Generic = (GenericBase)gmm
                    .GetConstructor(new[] { this.GetType() })
                    .Invoke(new object[] { this });

                try
                {
                    Generic.Init();
                }
                catch (TargetInvocationException)
                {
                    if (null != Generic.error)
                        throw Generic.error;
                    throw;
                }
            }
            try
            {
                return Generic.Eval(nodeToCheck);
            }
            catch (TargetInvocationException)
            {
                if (null != Generic.error)
                    throw Generic.error;
                throw;
            }
        }

        internal virtual Expression GetExpression(Expression node)
        {
            Expression @this = Expression.Constant(this);
            return Expression.Call(
                @this,
                this.GetType().GetMethod("Internal"),
                node);
        }

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
    }
}

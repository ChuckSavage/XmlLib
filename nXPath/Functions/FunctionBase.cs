using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace XmlLib.nXPath.Functions
{
    internal class FunctionBase
    {
        protected XPath_Part part;

        internal FunctionBase(XPath_Part part)
        {
            this.part = part;
        }

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
    }
}

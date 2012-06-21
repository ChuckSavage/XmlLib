// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    /// <summary>
    /// NodeSet Node() returns an XElement, XAttribute or an array of either.  So
    /// instead return this so we can know what it is.
    /// </summary>
    internal class NodeResult
    {
        internal enum eResultType
        {
            Attribute,
            AttributeArray,
            Element,
            ElementArray
        }

        internal XAttribute Attribute { get { return Result as XAttribute; } }

        internal XAttribute[] AttributeArray { get { return Result as XAttribute[]; } }

        internal XElement Element { get { return Result as XElement; } }

        internal XElement[] ElementArray { get { return Result as XElement[]; } }

        internal eResultType ResultType { get; private set; }

        internal object Result { get; private set; }

        internal NodeResult(object result, eResultType resultType)
        {
            if (null == result)
                throw new ArgumentNullException();
            Result = result;
            ResultType = resultType;
        }
    }
}
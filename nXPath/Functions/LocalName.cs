﻿// Copyright SeaRisen LLC
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
    /// <para>Example: root2.XPathElement("pair[local-name()={0}]", "div");</para>
    /// <para>Syntax: local-name()</para>
    /// <para>Syntax: local-name(nodeset)</para>
    /// <para>- nodeset: path to a node or attribute, and its local-name.</para>
    /// <para>Returns: Returns the name of the current node or the first node 
    /// in the specified node set - without the namespace prefix</para>
    /// </summary>
    internal class LocalName : FunctionBase
    {
        public LocalName(XPath_Part part)
            : base(part, typeof(LocalNameGeneric<>))
        {
            //_CompareAttribute = LocalName_CompareAttribute;
        }

        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsRequired { get { return false; } }
        /// <summary>
        /// false
        /// </summary>
        internal override bool ArgumentsValueRequired { get { return false; } }

        internal class LocalNameGeneric<T> : GenericBase
        {
            public LocalNameGeneric(LocalName name, XElement nodeToCheck)
                : base(nodeToCheck, name.part)
            {
            }

            bool IsEqual(XName xname)
            {
                if (null != xname)
                {
                    return xname.LocalName == part.Value.ToString();
                }
                return false;
            }

            public override bool Eval()
            {
                try
                {
                    if (string.IsNullOrEmpty(part.Key)) return IsEqual(node.Name);
                    NodeResult result = nodeset.Node(node, part.Key);
                    if (null == result) return false;
                    switch (result.ResultType)
                    {
                        case NodeResult.eResultType.Attribute:
                            return IsEqual(result.Attribute.Name);

                        case NodeResult.eResultType.AttributeArray:
                            return result.AttributeArray
                                .Any(a => IsEqual(a.Name));

                        case NodeResult.eResultType.Element:
                            return IsEqual(result.Element.Name);

                        case NodeResult.eResultType.ElementArray:
                            return result.ElementArray
                                .Any(e => IsEqual(e.Name));
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                return false;
            }
        }
    }
}
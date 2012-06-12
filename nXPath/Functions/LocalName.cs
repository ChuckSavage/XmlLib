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
            LocalName self;

            public LocalNameGeneric(LocalName name, XElement nodeToCheck)
                : base(nodeToCheck, name.part)
            {
                self = name;
            }

            bool IsEqual(object node)
            {
                XName xname = Name(node);
                if (null != xname)
                {
                    return xname.LocalName == part.Value.ToString();
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
    }
}
// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    class HasNode: FunctionBase
    {
        public HasNode(XPath_Part part)
            : base(part, typeof(HasNodeGeneric<>))
        {
        }

        internal class HasNodeGeneric<T> : GenericBase
        {
            HasNode self;

            public HasNodeGeneric(HasNode name, XElement nodeToCheck)
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
                    object result = nodeset.Node(node, part.Key);
                    if (null == result) return false;
                    IEnumerable<object> list = result as IEnumerable<object>;
                    if (null == list)
                        return true;
                    return list.Count() > 0;
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

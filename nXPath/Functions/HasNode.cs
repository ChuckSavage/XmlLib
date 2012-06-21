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
    class HasNode : FunctionBase
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

            public override bool Eval()
            {
                try
                {
                    NodeResult result = nodeset.Node(node, part.Key);
                    if (null == result) return false;
                    switch (result.ResultType)
                    {
                        case NodeResult.eResultType.AttributeArray:
                            return result.AttributeArray.Length > 0;

                        case NodeResult.eResultType.ElementArray:
                            return result.ElementArray.Length > 0;

                        default: // is an XAttribute or XElement, meaning it has the node
                            return true;
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
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
    /// <summary>
    /// <para>Example: root2.XPathElement("pair[max(@Key, {0})]", 0);</para>
    /// <para>Syntax: max(key, type)</para>
    /// <para>- key: is element or attribute path nodeset</para>
    /// <para>- type: isn't a value to compare against, but the type of the key to get 
    /// the min/max value of.</para>
    /// <para>Returns: The min/max pair node.</para>
    /// </summary>
    internal class MinMax : FunctionBase
    {
        enum eMinMax
        {
            Min, Max
        }
        readonly eMinMax Function;

        internal MinMax(XPath_Part part, string function)
            : base(part, typeof(GenericMinMax<>))
        {
            if (function.StartsWith("min"))
                Function = eMinMax.Min;
            else
                Function = eMinMax.Max;
        }

        internal class GenericMinMax<T> : GenericBase
        {
            IEnumerable<XElement> nodes;
            T max;
            MinMax self;

            public GenericMinMax(MinMax mm, XElement nodeToCheck)
                :base(nodeToCheck, mm.part)
            {
                self = mm;
            }

            public override bool Eval()
            {
                try
                {
                    T[] values;
                    if (nodeset.NodeValue(node, out values))
                        return values.Any(v => Compare<T>.Equal(v, max));
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
                    nodes = node.Parent.Elements(node.Name)
                        .Where(x => null != nodeset.Node(x, part.Key)).ToList();

                    if (self.Function == eMinMax.Max)
                        max = nodes.Max(x => { T[] value; nodeset.NodeValue(x, out value); return value.First(); });
                    else
                        max = nodes.Min(x => { T[] value; nodeset.NodeValue(x, out value); return value.First(); });
                }
                catch (Exception ex) { error = ex; }
            }
        }
    }
}
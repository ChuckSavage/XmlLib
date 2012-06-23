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
    internal class KVP : FunctionBase
    {
        internal KVP(XPath_Part part)
            : base(part, typeof(KVPGeneric<>))
        {
        }

        /// <summary>
        /// Bool result of NotEqual, LessThanOrEqual, etc of any part.Value in values.
        /// </summary>
        internal static bool Eval<T>(XPath_Part part, T[] values)
        {
            T value = (T)part.Value;
            return values.Any(v =>
            {
                if (part.NotEqual)
                    return Compare<T>.NotEqual(v, value);
                else if (part.LessThanOrEqual)
                    return Compare<T>.LessThanOrEqual(v, value);
                else if (part.GreaterThanOrEqual)
                    return Compare<T>.GreaterThanOrEqual(v, value);
                else if (part.Equal)
                    return Compare<T>.Equal(v, value);
                else if (part.LessThan)
                    return Compare<T>.LessThan(v, value);
                else if (part.GreaterThan)
                    return Compare<T>.GreaterThan(v, value);
                return false;
            });
        }

        internal class KVPGeneric<T> : GenericBase
        {
            public KVPGeneric(KVP kvp)
                :base(kvp.part)
            {
            }

            public override bool Eval(XElement node)
            {
                try
                {
                    T[] values;
                    if (nodeset.NodeValue(node, out values))
                        return KVP.Eval(part, values);
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
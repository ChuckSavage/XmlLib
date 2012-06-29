// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    internal class StringGeneric<T> : GenericBase
    {
        FunctionBase function;
        Func<string, string, bool> compare;
        bool? isUpper = false;
        string pattern;
        bool comparePatternWithValue;

        public StringGeneric(Func<string, string, bool> compare, FunctionBase func)
            : base(func.part)
        {
            this.compare = compare;
            function = func;
        }

        bool Eval(string value)
        {
            if (true == isUpper)
                value = value.ToUpperInvariant();
            else if (false == isUpper)
                value = value.ToLowerInvariant();
            if(comparePatternWithValue)
                return compare(pattern, value);
            return compare(value, pattern);
        }

        public override bool Eval(XElement node)
        {
            try
            {
                T[] values;
                if (nodeset.NodeValue(node, out values) && values.Length > 0)
                    return values.Any(v => Eval(v.ToString()));
            }
            catch (Exception ex)
            {
                error = ex;
            }
            return false;
        }

        public override void Init()
        {
            if (function.Args.Length > 2)
            {
                object arg = function.Args[2];
                if (arg is bool)
                    isUpper = (bool)arg;
                else if (arg is string)
                {
                    bool temp;
                    if (bool.TryParse(arg.ToString(), out temp))
                        isUpper = temp;
                }
            }

            pattern = part.Value.ToString();
            if (true == isUpper)
                pattern = pattern.ToUpperInvariant();
            else if (false == isUpper)
                pattern = pattern.ToLowerInvariant();

            comparePatternWithValue = null != function.NodeSet && 1 == function.NodeSet.Index;
        }
    }
}
// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Xml.Linq;

namespace XmlLib.nXPath.Functions
{
    internal abstract class GenericBase
    {
        public Exception error;
        protected XPath_Part part;
        protected NodeSet nodeset;

        public GenericBase(XPath_Part part)
        {
            this.part = part;
            nodeset = new NodeSet(part);
        }

        public abstract bool Eval(XElement node);

        public virtual void Init()
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlLib.nXPath
{
    public class NodeSet
    {
        // this really should be a separate class, but for quick add, using this class with the name I want
        string nodeset;
        public NodeSet(string nodeset)
        {
            this.nodeset = nodeset;
        }
        public int Index { get; private set; }

        public static NodeSet ParseArgs(object[] args)
        {
            NodeSet nodeSet = null; int index = 0;
            foreach (object arg in args)
            {
                if (arg is NodeSet)
                {
                    nodeSet = (NodeSet)arg;
                    nodeSet.Index = index;
                    return nodeSet;
                }
                if (++index >= 2) break;
            }
            return null;
        }

        public override string ToString() { return nodeset; }
        public override int GetHashCode() { return nodeset.GetHashCode(); }

        public static implicit operator string(NodeSet x) { return x.nodeset; }
    }
}
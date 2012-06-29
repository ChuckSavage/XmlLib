// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

namespace XmlLib.nXPath
{
    /// <summary>
    /// String wrapper class to diferentiate between parameters to functions, which
    /// is the key(nodeset) and which is the value to compare versus.
    /// </summary>
    public class NodeSet
    {
        string nodeset;
        public NodeSet(string nodeset)
        {
            this.nodeset = nodeset;
        }

        public int Index { get; private set; }

        internal static NodeSet ParseArgs(object[] args)
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
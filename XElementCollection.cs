// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Linq;
using System.Xml.Linq;

namespace XmlLib
{
    public class XElementCollection
    {
        protected XElement self;
        protected string child;
        protected Func<XElement, XElement, bool> comparer;
        protected bool duplicates;

        public XElementCollection(XElement self, string childTagName, Func<XElement, XElement, bool> comparer, bool permitDuplicates)
        {
            this.self = self;
            this.child = childTagName;
            this.comparer = comparer;
            this.duplicates = permitDuplicates;
        }

        public void Add(XElement item)
        {
            if (!duplicates)
                Remove(item);
            self.Add(item);
        }

        public int Count { get { return Items.Length; } }

        public XElement Find(Func<XElement, bool> compare)
        {
            return self.GetEnumerable(child, x => x)
                       .FirstOrDefault(x => compare(x));
        }

        protected XElement FindInternal(XElement item)
        {
            return self.GetEnumerable(child, x => x)
                       .FirstOrDefault(x => comparer(x, item));
        }

        public XElement[] Items
        {
            get { return self.GetEnumerable(child, x => x).ToArray(); }
        }

        public void Remove(XElement item)
        {
            XElement find = FindInternal(item);
            if (null != find)
                find.Remove();
        }
    }
}

// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XmlLib.nXPath
{
    public class cXPath
    {
        private static IEnumerable<XElement> ParseInternal(XElement contextNode, XPathString part)
        {
            IEnumerable<XElement> elements = PartToElements(contextNode, part);
            foreach (XPath_Bracket bracket in part.Brackets)
                elements = bracket.Elements(elements);

            return elements;
        }

        private static IEnumerable<XElement> PartToElements(XElement contextNode, XPathString xp)
        {
            string name = xp.Name;
            bool star = false;
            if (star = name.Contains('*'))
            {
                string[] parts = name.Split('*');
                if (parts.Length > 2)
                    throw new ArgumentException("Invalid name: " + name);
                name = parts[0];
            }
            IEnumerable<XElement> elements;
            if (xp.IsElements)
            {
                if (string.IsNullOrEmpty(name) || star)
                    elements = contextNode.Elements();
                else
                    elements = contextNode.GetElements(name);
            }
            else
            {
                if (string.IsNullOrEmpty(name) || star)
                    elements = contextNode.Descendants();
                else
                    elements = contextNode.GetDescendants(name);
            }
            if (star && !string.IsNullOrEmpty(name))
                elements = elements.Where(x => x.Name.LocalName.StartsWith(name));
            return elements;
        }

        /// <summary>
        /// Get the elements of a path "path/to/node" or "path[to/node/@attribute>=20.50]".
        /// <remarks>
        /// <para>See XPath docs for help on using [number][key=value]
        /// - syntax (http://www.w3.org/TR/xpath/)</para>
        /// </remarks>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <param name="create">create path if it doesn't exist?</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static IEnumerable<XElement> Enumerable(XElement source, XPathString path, bool create)
        {
            IEnumerable<XElement> enumerableXElement;

            if (null == path)
                throw new ArgumentNullException("Path cannot be null.");

            if (!path.IsRelative)
                source = source.Root();
            List<XElement> list = new[] { source }.ToList();
            for (int i = 0; i < path.PathSegments.Length; i++)
            {
                List<XElement> newList = new List<XElement>();
                XPathString xpathString = path.PathSegments[i];
                //bool last = (i + 1) == path.PathSegments.Length;

                if (xpathString.IsXPath)
                {
                    bool foundOne = false;
                    list.ForEach(xElement =>
                    {
                        enumerableXElement = ParseInternal(xElement, xpathString);
                        if (null != enumerableXElement)
                        {
                            newList.AddRange(enumerableXElement.ToList());
                            foundOne = true;
                        }
                    });
                    if (foundOne)
                    {
                        list = newList;
                        continue;
                    }
                }
                list.ForEach(xElement => newList.AddRange(PartToElements(xElement, xpathString)));
                list = newList;
            }

            // If the node wasn't found and we were asked to create it, do the best we can
            // If it is a simple search, then we won't be able to place the node and it will be dangling (without a parent node)
            if (list.Count == 0 && create)
            {
                // Find parent element if we can, and plug in the new node there
                XElement parent = null;
                foreach (var seg in path.PathSegments.Take(path.PathSegments.Length - 1))
                    parent = (parent ?? source).XPathElement(seg, create);
                // If not found, try finding a sibling without search parameters
                // and use it's parent.
                if (null == parent)
                {
                    XPathString clean = path.Clone_NoBrackets();
                    XElement sibling = source.XPathElement(clean, false);
                    if (null != sibling)
                        parent = sibling.Parent;
                }

                // Using last segment in XPath, create the node that is missing
                var segment = path.PathSegments.Last();
                XElement node = new XElement(segment.Name);
                if (null != parent)
                    parent.Add(node);

                // Add any child element's or attribute's that were requested in the search
                foreach (var b in segment.Brackets)
                {
                    foreach (var part in b.Parts)
                    {
                        if (!part.Equal) continue;
                        if (part.IsValueAttribute)
                            node.Add(new XAttribute(part.Key, part.Value));
                        else
                            node.Add(new XElement(part.Key, part.Value));
                    }
                }
                list.Add(node);
            }
            return list;
        }
    }
}
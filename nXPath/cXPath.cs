// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                if (!xp.IsRelative)
                    contextNode = contextNode.Root();
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
        /// <param name="create">create path if it doesn't exist?</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static IEnumerable<XElement> Enumerable(XElement source, XPathString path, bool create)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null.");

            IEnumerable<XElement> enumerableXElement;
            List<XElement> list = new[] { source }.ToList();
            XElement result;
            for (int i = 0; i < path.PathSegments.Length; i++)
            {
                List<XElement> newList = new List<XElement>();
                XPathString xpathString = path.PathSegments[i];
                bool last = (i + 1) == path.PathSegments.Length;

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
                string part = xpathString.Text.Split('[')[0];
                if (last)
                    list.ForEach(xElement =>
                    {
                        if (xpathString.IsElements)
                            newList.AddRange(xElement.GetElements(part));
                        else
                            newList.AddRange(xElement.GetDescendants(part));
                    });
                else
                    list.ForEach(xElement =>
                    {
                        if (create)
                            result = xElement.GetElement(part);
                        else
                            result = xElement.Element(xElement.ToXName(part));
                        if (null != result)
                            newList.Add(result);
                    });

                list = newList;
            }
            return list;
        }
    }
}
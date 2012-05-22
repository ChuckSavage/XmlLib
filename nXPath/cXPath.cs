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
        public cXPath()
        {
        }

        private XPath_Bracket[] GetBrackets(XElement contextNode, XPathString part, out string name)
        {
            name = part.Text;
            if (part.Text.Contains('['))
            {
                Match[] matches = Regex.Matches(part.Format, @"\[[^]]*\]")
                       .Cast<Match>()
                       .ToArray();
                if (matches.Length > 0)
                {
                    name = part.Format.Remove(matches[0].Index);
                    List<string> list = new List<string>() { name };
                    list.AddRange(matches.Select(m => m.Value));
                    XPathString[] parts = part.ToPaths(list);
                    XPath_Bracket[] result = parts
                           .Skip(1)
                           .Select(xps => new XPath_Bracket(xps))
                           .ToArray();
                    return result;
                }
            }
            return null;
        }

        private IEnumerable<XElement> ParseInternal(XElement contextNode, XPathString part)
        {
            string name;
            bool star = false;
            XPath_Bracket[] brackets = GetBrackets(contextNode, part, out name);
            if (star = name.Contains('*'))
            {
                string[] parts = name.Split('*');
                if (parts.Length > 2)
                    throw new ArgumentException("Invalid name: " + name);
                name = parts[0];
            }
            IEnumerable<XElement> elements;
            if (part.IsElements)
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

            if (null != brackets)
                foreach (XPath_Bracket bracket in brackets)
                {
                    elements = bracket.Elements(contextNode, elements);
                }
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

            IEnumerable<XElement> e;
            List<XElement> list = new[] { source }.ToList();
            XElement result;
            for (int i = 0; i < path.PathSegments.Length; i++)
            {
                List<XElement> newList = new List<XElement>();
                XPathString xp = path.PathSegments[i];
                bool last = (i + 1) == path.PathSegments.Length;

                if (xp.IsXPath)
                {
                    bool foundOne = false;
                    list.ForEach(x =>
                    {
                        e = new cXPath().ParseInternal(x, xp);
                        if (null != e)
                        {
                            newList.AddRange(e);
                            foundOne = true;
                        }
                    });
                    if (foundOne)
                    {
                        list = newList;
                        continue;
                    }
                }
                string part = xp.Text.Split('[')[0];
                if (last)
                    list.ForEach(x =>
                    {
                        if (xp.IsElements)
                            newList.AddRange(x.GetElements(part));
                        else
                            newList.AddRange(x.GetDescendants(part));
                    });
                else
                    list.ForEach(x =>
                    {
                        if (create)
                            result = x.GetElement(part);
                        else
                            result = x.Element(x.ToXName(part));
                        if (null != result)
                            newList.Add(result);
                    });

                list = newList;
            }
            return list;
        }

        /// <summary>
        /// Get the first element of a path "path/to/node" or "path[to/node/@attribute>=20.50]".
        /// <remarks>
        /// <para>See XPath docs for help on using [number][key=value]
        /// - syntax (http://www.w3.org/TR/xpath/)</para>
        /// </remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static XElement Element(XElement source, XPathString path, bool create)
        {
            return Enumerable(source, path, create).FirstOrDefault();
        }
    }
}
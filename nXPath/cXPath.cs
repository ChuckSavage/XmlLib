﻿// Copyright SeaRisen LLC
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
            XPath_Bracket[] brackets = GetBrackets(contextNode, part, out name);
            IEnumerable<XElement> elements;
            if (part.IsElements)
                elements = contextNode.GetElements(name);
            else
                elements = contextNode.GetDescendants(name);
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

            XElement result = source;
            for (int i = 0; i < path.PathSegments.Length; i++)
            {
                XPathString xp = path.PathSegments[i];
                bool last = (i + 1) == path.PathSegments.Length;
                if (xp.IsXPath)
                {
                    var e = new cXPath().ParseInternal(result, xp);
                    if (last)
                        return e;

                    XElement temp = e.FirstOrDefault();
                    if (null != temp)
                    {
                        result = temp;
                        continue;
                    }
                }
                string part = xp.Text;
                if (last)
                    if (xp.IsElements)
                        return result.GetElements(part);
                    else
                        return result.GetDescendants(part);
                if (create)
                    result = result.GetElement(part);
                else
                {
                    result = result.Element(result.ToXName(part));
                    if (null == result)
                        throw new ArgumentOutOfRangeException(part);
                }
            }
            return new XElement[] { result };
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
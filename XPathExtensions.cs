// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlLib.nXPath;

namespace XmlLib
{
    public static class XPathExtensions
    {
        /// <summary>
        /// Navigate to a specific path within source.  (create path if it doesn't exist?)
        /// <remarks>See XPath docs for help on using [number][key=value] 
        /// syntax (http://www.w3.org/TR/xpath/)</remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static IEnumerable<XElement> XPath(this XElement source, XPathString path, bool create)
        {
            return cXPath.Enumerable(source, path, create);
        }

        /// <summary>
        /// Navigate to a specific path within source, create it if it doesn't exist.
        /// <remarks>See XPath docs for help on using [number][key=value] 
        /// syntax (http://www.w3.org/TR/xpath/)</remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static IEnumerable<XElement> XPath(this XElement source, string path, params object[] args)
        {
            return XPath(source, new XPathString(path, args), true);
        }

        /// <summary>
        /// Navigate to a specific path within source.  (create path if it doesn't exist?)
        /// <remarks>See XPath docs for help on using [number][key=value] 
        /// syntax (http://www.w3.org/TR/xpath/)</remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static XElement XPathElement(this XElement source, XPathString path, bool create)
        {
            return XPath(source, path, create).FirstOrDefault();
        }

        /// <summary>
        /// Navigate to a specific path within source, create it if it doesn't exist.
        /// <remarks>See XPath docs for help on using [number][key=value] 
        /// syntax (http://www.w3.org/TR/xpath/)</remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static XElement XPathElement(this XElement source, string path, params object[] args)
        {
            return XPathElement(source, new XPathString(path, args), true);
        }

    }
}

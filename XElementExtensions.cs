// Copyright SeaRisen LLC
// You may use this code without restrictions, but keep the copyright notice with this code.
// This file is found at: https://github.com/ChuckSavage/XmlLib
// If you find this code helpful and would like to donate, please consider purchasing one of
// the products at http://products.searisen.com, thank you.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XmlLib
{
    /// <summary>
    /// XElement extension methods.
    /// </summary>
    public static class XElementExtensions
    {
        public const bool ATTRIBUTE = true;
        public const bool ELEMENT = false;

        #region GetElement
        /// <summary>
        /// Get child element, create it if it doesn't exist in source.  
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static XElement GetElement(this XElement source, XName name)
        {
            XElement xe = null;
            if (source.HasElements)
            {
                xe = source.Element(name);
                if (null == xe) // get first node that matches by tag name (if one exists)
                    xe = source.Elements()
                        .FirstOrDefault(x => x.Name.LocalName == name.LocalName);
            }
            if (null == xe)
                source.Add(xe = new XElement(name));
            return xe;
        }

        /// <summary>
        /// Get child element, create it if it doesn't exist in source.  
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static XElement GetElement(this XElement source, string name)
        {
            if (name.Contains('/') || name.Contains('\\'))
                return Path(source, name);
            return GetElement(source, ToXName(source, name));
        }

        /// <summary>
        /// Get child elements.
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static IEnumerable<XElement> GetElements(this XElement source, XName name)
        {
            if (source.HasElements)
            {
                if (null == name)
                    return source.Elements();
                IEnumerable<XElement> elements = source.Elements(name);
                if (0 == elements.Count())
                    elements = source.Elements()
                        .Where(x => x.Name.LocalName == name.LocalName);
                return elements;
            }
            return new XElement[] { };
        }

        /// <summary>
        /// Get child elements.
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static IEnumerable<XElement> GetElements(this XElement source, string name)
        {
            if (source.HasElements)
            {
                if (string.IsNullOrEmpty(name))
                    return source.Elements();
                source = NameCheck(source, name, out name);
                return GetElements(source, ToXName(source, name));
            }
            return new XElement[] { };
        }

        /// <summary>
        /// Get descendant elements.
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static IEnumerable<XElement> GetDescendants(this XElement source, XName name)
        {
            if (null == name)
                return source.Descendants();
            IEnumerable<XElement> elements = source.Descendants(name);
            if (0 == elements.Count())
                elements = source.Descendants()
                    .Where(x => x.Name.LocalName == name.LocalName);
            return elements;
        }

        /// <summary>
        /// Get descendant elements.
        /// </summary>
        /// <param name="name">The tag name of the XElement.</param>
        /// <returns>XElement found or created.</returns>
        public static IEnumerable<XElement> GetDescendants(this XElement source, string name)
        {
            if (string.IsNullOrEmpty(name))
                return source.Descendants();
            source = NameCheck(source, name, out name);
            return GetDescendants(source, ToXName(source, name));
        }

        /// <summary>
        /// Try and get the element specified by the name path, 
        /// if it exists return true and out the value.
        /// </summary>
        public static bool TryGetElement
        (
            this XElement source, 
            string name, 
            out XElement value
        )
        {
            try
            {
                value = Path(source, name, false);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                value = null;
                return false;
            }
        }
        #endregion

        #region GetString
        /// <summary>
        /// Get STRING of element/attribute
        /// </summary>
        public static string GetString
        (
            this XElement source, 
            XName name, 
            string defaultValue
        )
        {
            if (null == source)
                return defaultValue;
            string result;
            if (null == name)
                result = (string)source;
            else
            {
                if (name.Namespace == source.Name.Namespace)
                    result = (string)source.Attribute(name.LocalName);
                else
                    result = (string)source.Attribute(name);
                if (null == result)
                    result = (string)source.Element(name);
            }
            if (null == result)
                result = defaultValue;
            return result;
        }

        /// <summary>
        /// Get STRING of element/attribute
        /// </summary>
        public static string GetString
        (
            this XElement source, 
            string name, 
            string defaultValue
        )
        {
            source = NameCheck(source, name, out name);
            return GetString(source, ToXName(source, name), defaultValue);
        }
        #endregion

        #region Get
        /// <summary>
        /// Generic Get for a type.
        /// <remarks>
        /// It works as long as there is a converter for the type to 
        /// convert from string.
        /// </remarks>
        /// </summary>
        /// <returns>The element converted to its type or the default 
        /// if it didn't exist or was empty.</returns>
        public static T Get<T>(this XElement source, XName name, T @default)
        {
            T value;
            string sValue = GetString(source, name, null);
            if (string.IsNullOrEmpty(sValue))
                return @default;
            if (ConverterCache<T>.TryParse(sValue, out value))
                return value;
            if (ParseCache<T>.TryParse(sValue, out value))
                return value;
            if (ConstructorCache<T>.TryInvoke(source, sValue, out value))
                return value;
            // Throwing an exception helps you improve your parsing if the default
            // methods provided by the type creator are not sufficient.  You can
            // create a constructor for your class or a parsing class for types that
            // will handle the string value or the XElement itself.
            throw new NotSupportedException("Can't parse XElement value to type:" + typeof(T).Name);
            //return @default;
        }

        /// <summary>
        /// Generic Get for a type.
        /// <remarks>
        /// It works as long as there is a converter for the type to convert 
        /// from string.
        /// </remarks>
        /// </summary>
        /// <returns>The element converted to its type or the default if it 
        /// didn't exist or was empty.</returns>
        public static T Get<T>(this XElement source, string name, T @default)
        {
            source = NameCheck(source, name, out name);
            return Get(source, ToXName(source, name), @default);
        }

        // If TypeConverter is unavailable (aka Windows Phone 7),
        /// <summary>
        /// Store converters for a quicker lookup.
        /// <remarks>
        /// Saves about 3 MS (averaged) on ~3000 Get()'s 
        /// run 1000 times.
        /// </remarks>
        /// </summary>
        private static class ConverterCache<T>
        {
            public static bool TryParse(string sValue, out T value)
            {
                value = default(T);
                // If TypeConverter is unavailable (aka Windows Phone 7),
#if !WindowsPhone7
                try
                {
                    value = (T)Converter.ConvertFromString(sValue);
                    return true;
                }
                catch (NotSupportedException)
                { }
#endif
                return false;
            }
#if !WindowsPhone7
            public static readonly TypeConverter Converter;
            static ConverterCache()
            { Converter = TypeDescriptor.GetConverter(typeof(T)); }
#endif
        }
        #endregion

        #region Set
        /// <summary>
        /// Set any value via its .ToString() method.
        /// <para>
        /// Returns XElement of source or the new XElement if is an ELEMENT
        /// </para>
        /// </summary>
        /// <param name="isAttribute">
        /// Use ATTRIBUTE or ELEMENT for clarity
        /// </param>
        /// <returns>source or XElement value</returns>
        public static XElement Set
        (
            this XElement source, 
            string name, 
            object value, 
            bool isAttribute
        )
        {
            return Set(source, name, value, isAttribute, false);
        }

        /// <summary>
        /// Set any value via its .ToString() method.
        /// <para>
        /// Returns XElement of source or the new XElement if is an ELEMENT
        /// </para>
        /// </summary>
        /// <param name="isAttribute">
        /// Use ATTRIBUTE or ELEMENT for clarity
        /// </param>
        /// <returns>source or XElement value</returns>
        public static XElement Set
        (
            this XElement source, 
            string name, 
            object value, 
            bool isAttribute, 
            bool preserveChildren
        )
        {
            source = NameCheck(source, name, out name);
            XName xname = ToXName(source, name);
            return Set(source, xname, value, isAttribute, preserveChildren);
        }

        /// <summary>
        /// Set any value via its .ToString() method.
        /// <para>
        /// Returns XElement of source or the new XElement if is an ELEMENT
        /// </para>
        /// </summary>
        /// <param name="isAttribute">
        /// Use ATTRIBUTE or ELEMENT for clarity
        /// </param>
        /// <returns>source or XElement value</returns>
        public static XElement Set
        (
            this XElement source, 
            XName name, 
            object value, 
            bool isAttribute
        )
        {
            return Set(source, name, value, isAttribute, false);
            //< false aka don't preserve children of element set.
            //< XElement default behavoir.
        }

        /// <summary>
        /// Set any value via its .ToString() method.
        /// <para>
        /// Returns XElement of source or the new XElement if is an ELEMENT
        /// </para>
        /// </summary>
        /// <param name="isAttribute">
        /// Use ATTRIBUTE or ELEMENT for clarity
        /// </param>
        /// <returns>source or XElement value</returns>
        public static XElement Set
        (
            this XElement source, 
            XName name, 
            object value, 
            bool isAttribute, 
            bool preserveChildren
        )
        {
            // If value is null or empty, ignore / remove old
            bool delete = null == value;
            bool modified = false;
            string sValue = string.Empty;
            if (null != value)
            {
                sValue = value.ToString();
                delete |= string.IsNullOrEmpty(sValue);
            }
            XElement eValue = null, result = source;
            XAttribute aValue = null;

            if (null == name)
                eValue = source;
            else
            {
                eValue = source.Element(name);
                if (name.Namespace == source.Name.Namespace)
                    aValue = source.Attribute(name.LocalName);
                else
                    aValue = source.Attribute(name);
            }

            // If changing from Attribute to Element or visa-versa, remove old
            if (null != eValue && isAttribute)
            {
                eValue.Remove();
                eValue = null;
                modified = true;
            }
            else if (null != aValue && !isAttribute)
            {
                aValue.Remove();
                aValue = null;
                modified = true;
            }
            // If has an Element tag by the name, replace / remove
            if (null != eValue)
            {
                if (delete)
                    eValue.Remove();
                else
                {
                    // Setting a 'value' to an element, destroys any children
                    // preserve the child elements!
                    if (preserveChildren && eValue.HasElements)
                    {
                        var children = eValue.Elements().ToArray();
                        eValue.Value = sValue; // removes any children
                        foreach (XElement child in children) // re-add child elements
                            eValue.Add(child);
                    }
                    else
                        eValue.Value = sValue;
                    result = eValue;
                }
                modified = true;
            }
            // else if has an Attribute tag by the name, replace / remove
            else if (null != aValue)
            {
                if (delete)
                    aValue.Remove();
                else
                    aValue.Value = sValue;
                modified = true;
            }
            // else if value passed wasn't empty, add Attribute or Element
            else if (!delete)
            {
                if (isAttribute)
                {
                    if (name.Namespace == source.Name.Namespace)
                        source.Add(new XAttribute(name.LocalName, sValue));
                    else
                        source.Add(new XAttribute(name, sValue));
                }
                else
                    source.Add(result = new XElement(name, sValue));
                modified = true;
            }
            if (modified)
                source.SetSave();
            return result;
        }
#endregion

        #region Enumerable
        /// <summary>
        /// Convert a list of like tags into an enumerable collection.
        /// </summary>
        public static IEnumerable<T> GetEnumerable<T>
        (
            this XElement source, 
            XName name, 
            Func<XElement, T> convert
        )
        {
            if (null == name)
                return GetEnumerable(source, convert);
            return GetElements(source, name).Select(x => convert(x));
        }

        /// <summary>
        /// Convert a list of like tags into an enumerable collection.
        /// </summary>
        public static IEnumerable<T> GetEnumerable<T>
        (
            this XElement source, 
            string name, 
            Func<XElement, T> convert
        )
        {
            if (string.IsNullOrEmpty(name))
                return GetEnumerable(source, convert);
            source = NameCheck(source, name, out name);
            return GetEnumerable(source, ToXName(source, name), convert);
        }

        /// <summary>
        /// Convert a list of like tags into an enumerable collection.
        /// </summary>
        public static IEnumerable<T> GetEnumerable<T>
        (
            this XElement source, 
            Func<XElement, T> convert
        )
        {
            return source.Elements().Select(x => convert(x));
        }

        /// <summary>
        /// Convert an enumerable collection to a list of XElement tags.
        /// </summary>
        /// <returns>Element that collection was set as children of.</returns>
        public static XElement SetEnumerable<T>
        (
            this XElement source,
            string path,
            IEnumerable<T> list,
            Func<T, XElement> convert
        )
        {
            return SetEnumerable(Path(source, path, true), list, convert);
        }

        /// <summary>
        /// Convert an enumerable collection to a list of XElement tags.
        /// </summary>
        /// <returns>Element that collection was set as children of.</returns>
        public static XElement SetEnumerable<T>
        (
            this XElement source,
            IEnumerable<T> list,
            Func<T, XElement> convert
        )
        {
            XElement[] elements = list.Select(t => convert(t)).ToArray();
            XElement first = elements.FirstOrDefault() ?? convert(default(T));
            source.GetElements(first.Name).Remove();
            if (elements.Length > 0)
                source.Add(elements);
            source.SetSave(); 
            return source;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// If name contains path information to a node, figure it out.
        /// </summary>
        internal static XElement NameCheck
        (
            this XElement source,
            string name,
            out string nameResult
        )
        {
            nameResult = name;
            if (!string.IsNullOrEmpty(name))
                if (name.Contains('\\') || name.Contains('/'))
                {
                    string[] parts = name.Split('\\', '/');
                    string path = string.Join("/", parts.Take(parts.Length - 1).ToArray());
                    source = Path(source, path, true);
                    nameResult = parts[parts.Length - 1];
                }
            return source;
        }

        /// <summary>
        /// Navigate to a specific path within source.  (create path if it doesn't exist?)
        /// </summary>
        public static XElement Path(this XElement source, string path, bool create)
        {
            if (null == path)
                throw new ArgumentNullException("Path cannot be null!");
            string[] parts = path.Split('\\', '/');
            XElement result = source;
            foreach (string part in parts)
            {
                if (create)
                    result = result.GetElement(part);
                else
                {
                    result = result.Element(ToXName(result, part));
                    if (null == result)
                        throw new ArgumentOutOfRangeException(part);
                }
            }
            return result;
        }

        /// <summary>
        /// Navigate to a specific path within source.  Creates the path if it doesn't exist.
        /// </summary>
        public static XElement Path(this XElement source, string path)
        {
            return Path(source, path, true);
        }

        /// <summary>
        /// Return root node of Xml tree of node that calls this
        /// </summary>
        /// <param name="source"></param>
        /// <returns>the root element</returns>
        public static XElement Root(this XElement source)
        {
            XElement root = source;
            while (null != root.Parent)
                root = root.Parent;
            return root;
        }

#if !SetSave
        /// <summary>
        /// Set a save value in the root node so that the containing xml file class
        /// knows that the file has changed, and therefore saves the changes.
        /// Instead of saving the file everytime even when no changes were made.
        /// <remarks>
        /// This is an intentionally blank stub so that the file compiles.
        /// </remarks>
        /// </summary>
        public static void SetSave(this XElement source)
        { }
#endif

        /// <summary>
        /// Convert a name like "im:big" to 
        /// source.GetNamespaceOfPrefix("im") + "big".
        /// </summary>
        public static XName ToXName(this XElement source, string name)
        {
            if (null != source && !string.IsNullOrEmpty(name))
            {
                XNamespace ns = source.Name.Namespace;
                if (name.Contains(':'))
                {
                    string[] parts = name.Split(':');
                    if (parts.Length > 2)
                        throw new ArgumentException("Invalid name: " + name);
                    ns = source.GetNamespaceOfPrefix(parts[0]);
                    name = parts[1];
                }
                return ns + name;
            }
            return null;
        }
        #endregion

        #region Demo GetInt
        // Demo of creating specific type getter - in this case the Int32 type
        public static int GetInt(this XElement source, string name, int defaultValue)
        {
            // parse path names like: "path/path/element" 
            source = NameCheck(source, name, out name);
            return GetInt(source, ToXName(source, name), defaultValue);
        }

        public static int GetInt(this XElement source, XName name, int defaultValue)
        {
            int result;
            if (Int32.TryParse(GetString(source, name, null), out result))
                return result;
            return defaultValue;
        }
        #endregion
    }

    /// <summary>
    /// If using something like 'Windows Phone 7' that doesn't have access to
    /// the TypeConverter class, then using a TryParse is another option.
    /// <para>
    /// This class tries to find a TryParse method within the generic type T
    /// otherwise it will fail gracefully.
    /// </para>
    /// </summary>
    internal static class ParseCache<T>
    {
        public static bool TryParse(string sValue, out T value)
        {
            return func(sValue, out value);
        }
        delegate bool TryPattern(string sValue, out T value);
        private static readonly TryPattern func;
        static ParseCache()
        {
            MethodInfo method = typeof(T).GetMethod(
                "TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() });
            if (method == null)
            {
                if (typeof(T) == typeof(string))
                    func = delegate(string sValue, out T value) 
                        { value = (T)(object)sValue; return true; };
                else
                    func = delegate(string sValue, out T value) 
                        { value = default(T); return false; };
            }
            else
            {
                func = (TryPattern)Delegate.CreateDelegate(typeof(TryPattern), method);
            }
        }
    }

    /// <summary>
    /// Try to get a constructor that takes a single string value
    /// </summary>
    internal static class ConstructorCache<T>
    {
        public static ConstructorInfo StringInfo { get; private set; }
        public static ConstructorInfo XElementInfo { get; private set; }
        /// <summary>
        /// Try invoking a constructor that takes a single string value or
        /// a single XElement value.
        /// <remarks>
        /// Creating a custom class to parse values either by string or
        /// from the XElement source itself, can be a slick way to make this
        /// system work for you.
        /// </remarks>
        /// </summary>
        public static bool TryInvoke(XElement source, string value, out T result)
        {
            result = default(T);
            if (null != StringInfo)
            {
                result = (T)StringInfo.Invoke(new object[] { value });
                return true;
            }
            if (null != XElementInfo)
            {
                result = (T)XElementInfo.Invoke(new object[] { value });
                return true;
            }
            return false;
        }
        static ConstructorCache()
        {
            StringInfo = typeof(T).GetConstructor(new Type[] { typeof(string) });
            XElementInfo = typeof(T).GetConstructor(new Type[] { typeof(XElement) });
        }
    }
}
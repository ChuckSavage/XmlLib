using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.IO;
using XmlLib;

namespace XmlLib_Test
{
    /// <summary>
    /// Summary description for XPath_Functions
    /// </summary>
    [TestClass]
    public class XPath_Functions
    {
        XElement root;

        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            DirectoryInfo projectDir = new DirectoryInfo(@"..\..\..\..\XmlLib\TestProject");
            string file = Path.Combine(projectDir.FullName, "XMLFile1.xml");
            root = XElement.Load(file);
        }

        public XPath_Functions()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Function_GenericCount_Descendants_TwoDeep()
        {
            string path = "//*[Items/Item]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants()
                .Where(x => x.GetElements("Items/Item").Count() > 0)
                .ToArray();

            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Count_Descendants_TwoDeep()
        {
            string path = "//pair[Items/Item]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants("pair")
                .Where(x => x.GetElements("Items/Item").Count() > 0)
                .ToArray();

            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Count_Descendants()
        {
            string path = "//pair[Items]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants("pair")
                .Where(x => x.GetElements("Items").Count() > 0)
                .ToArray();

            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Count_Descendants_Attribute()
        {
            string path = "//pair[@Key]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants("pair")
                .Where(x => null != x.Attribute("Key"))
                .ToArray();

            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Count_Descendants_Attribute_TwoDeep()
        {
            string path = "//pair[Value2/@Key]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants("pair")
                .Where(x => null != x.GetElement("Value2").Attribute("Key"))
                .ToArray();

            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_EndsWith_Dot()
        {
            string path = "//Name[ends-with(.,'tin')]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants("Name")
                                      .Where(x => x.Value.EndsWith("tin"))
                                      .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_EndsWith_Star()
        {
            string path = "//*[ends-with(*,'tin')]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants()
                                      .Where(x => x.Elements().Any(xx => ((string)xx).EndsWith("tin")))
                                      .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_EndsWith_Path()
        {
            string path = "//*[ends-with(Item/Name,'tin')]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants()
                .Where(x => x.HasElements ? x.GetElements("Item/Name").Any(xx => ((string)xx).EndsWith("tin")) : false)
                                      .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_EndsWith_Attrib()
        {
            string path = "//*[ends-with(@Key,'2')]";
            object[] args = new object[] { };
            XElement[] expected = root.Descendants()
                .Where(x => //x.HasAttributes ? 
                    (null == x.Attribute("Key") ? false : x.Attribute("Key").Value.EndsWith("2"))
                //: false
                    )
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_EndsWith_AttribPath()
        {
            string path = "//*[ends-with(food/@size,{0})]";
            object[] args = new object[] { "2" };
            XElement[] expected = root.Descendants()
                .Where(a =>
                {
                    if (!a.HasElements) return false;
                    XElement x = a.GetElement("food");
                    if (!x.HasAttributes) return false;
                    return (null == x.Attribute("size") ? false : x.Attribute("size").Value.EndsWith("2"));
                })
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Last()
        {
            string path = "pair[last()]";
            object[] args = new object[] { };
            XElement expected = root.Elements("pair").LastOrDefault();
            XElement actual = XPathExtensions.XPathElement(root, path, args);
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void Function_LocalName()
        {
            string path = "//*[local-name()={0}]";
            object[] args = new object[] { "div" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Elements().Any(xx => xx.Name.LocalName == "div"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_LocalName_CurrentNode()
        {
            string path = "//*[local-name(.)={0}]";
            object[] args = new object[] { "div" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Name.LocalName == "div")
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_LocalName_Attribute()
        {
            string path = "//*[local-name(@id)={0}]";
            object[] args = new object[] { "id" };
            XElement[] expected = root.Descendants()
                .Where(x =>
                {
                    XName xname = x.ToXName("id");
                    XAttribute a = x.Attribute(xname);
                    if (null == a) return false;
                    return a.Name.LocalName == "id";
                })
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_LocalName_AttributeStar()
        {
            string path = "//*[local-name(@*)={0}]";
            object[] args = new object[] { "id" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Attributes().Any(xa => xa.Name.LocalName == "id"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Name()
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            string path = "//*[name()={0}]";
            object[] args = new object[] { ns + "div" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Elements().Any(xx => xx.Name == ns + "div"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Name_CurrentNode()
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            string path = "//*[name(.)={0}]";
            object[] args = new object[] { ns + "div" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Name == ns + "div")
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Name_Attribute()
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            string path = "//*[name(@id)={0}]";
            object[] args = new object[] { ns + "id" };
            XElement[] expected = root.Descendants()
                .Where(x =>
                {
                    XName xname = x.ToXName("id");
                    XAttribute a = x.Attribute(xname);
                    if (null == a) return false;
                    return a.Name == ns + "id";
                })
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Name_AttributeStar()
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            string path = "//*[name(@*)={0}]";
            object[] args = new object[] { ns + "id" };
            XElement[] expected = root.Descendants()
                .Where(x => x.Attributes().Any(xa => xa.Name == ns + "id"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_StartsWith()
        {
            string path = "details[starts-with(*,'ADC')]";
            object[] args = new object[] { };
            XElement[] expected = root.Elements("details")
                                      .Where(x => x.Elements().Any(xx => ((string)xx).StartsWith("ADC")))
                                      .ToArray();
            XElement[] actual = XPathExtensions.XPath(root, path, args).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Function_Max_Attribute()
        {
            string path = "pair[max(@Key, {0})]";
            object[] args = new object[] { int.MinValue };

            Func<XAttribute, int> aToInt = a => (int)(a ?? new XAttribute("xa", int.MinValue));
            Func<XElement, int> max = x => aToInt(x.Attribute("Key"));
            XElement expected = root.Elements("pair")
                .Where(x => aToInt(x.Attribute("Key")) == x.Parent.Elements(x.Name).Max(max))
                .First();

            try
            {
                XElement actual = XPathExtensions.XPathElement(root, path, args);
                Assert.AreEqual(expected, actual);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Function_Min_Element()
        {
            string path = "pair[min(Value2/Value, {0})]";
            object[] args = new object[] { int.MaxValue };

            Func<XElement, int> eToInt = x => string.IsNullOrEmpty(x.Value) ? int.MaxValue : (int)x;
            int min = root.Elements("pair").Min(x => eToInt(x.GetElement("Value2/Value")));
            XElement expected = root.Elements("pair")
                                    .Where(x => eToInt(x.GetElement("Value2/Value")) == min)
                                    .First();

            try
            {
                XElement actual = XPathExtensions.XPathElement(root, path, args);
                Assert.AreEqual(expected, actual);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}

using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib;
using XmlLib.nXPath;
using System.Collections.Generic;
using System;

namespace XmlLib_Test
{
    /// <summary>
    ///This is a test class for XPathExtensionsTest and is intended
    ///to contain all XPathExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XPathExtensionsTest
    {
        XElement root1;

        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            DirectoryInfo projectDir = new DirectoryInfo(@"..\..\..\..\XmlLib\TestProject");
            string file = Path.Combine(projectDir.FullName, "XMLFile1.xml");
            root1 = XElement.Load(file);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for XPathElement
        ///</summary>
        [TestMethod()]
        public void XPathElementTest1()
        {
            XElement source = root1; 
            string path = "pair[@Key>=2 and @Key<6][2]";
            object[] args = new object[] { };
            XElement expected = root1.Elements("pair")
                                     .Where(x => ((int)x.Attribute("Key")) >= 2 && ((int)x.Attribute("Key")) < 6)
                                     .ElementAt(1);
            XElement actual;
            actual = XPathExtensions.XPathElement(source, path, args);
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        /// <summary>
        ///A test for XPathElement
        ///</summary>
        [TestMethod()]
        public void XPathElementTest()
        {
            XElement source = root1; 
            XPathString path = new XPathString("pair[@Key={0}]/Items/Item[Name={1}]", 2, "Martin");
            bool create = false;
            XElement expected = source.Elements("pair")
                .Where(x => ((int)x.Attribute("Key")) == 2)
                .SelectMany(x => x.Element("Items").Elements("Item"))
                .Where(x => ((string)x.Element("Name")) == "Martin")
                .FirstOrDefault();
            XElement actual;
            actual = XPathExtensions.XPathElement(source, path, create);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for XPath
        ///</summary>
        [TestMethod()]
        public void XPath_RootDescendants()
        {
            XElement pair2 = root1.XPathElement("pair[@Key=2]");
            XElement[] expected = pair2.Root().Descendants("Item")
                .Where(x => x.Elements().Any(xx => xx.Value == "Mike"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(pair2, "//Item[Name='Mike']", null)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for XPath
        ///</summary>
        [TestMethod()]
        public void XPath_RelativeDescendants()
        {
            XElement pair2 = root1.XPathElement("pair[@Key=2]");
            XElement[] expected = pair2.Descendants("Item")
                .Where(x => x.Elements().Any(xx => xx.Value == "Mike"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(pair2, ".//Item[Name='Mike']", null)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for XPath
        ///</summary>
        [TestMethod()]
        public void XPath_RelativeElements()
        {
            XElement pair2 = root1.XPathElement("pair[@Key=2]");
            XElement[] expected = pair2.GetElements("Items/Item")
                .Where(x => x.Elements().Any(xx => xx.Value == "Mike"))
                .ToArray();
            XElement[] actual = XPathExtensions.XPath(pair2, "Items/Item[Name='Mike']", null)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// XGetElement 
        ///</summary>
        [TestMethod()]
        public void XGetElement_RootElement_DateValue()
        {
            XElement pair2 = root1.XPathElement("pair[@Key=2]");
            var actual = pair2.XGetElement("/Items/Item[Name='Mike']/Date", DateTime.MinValue);
            var expected = DateTime.Parse("5/4/2008"); // pair2's date is 5/4/2000
            Assert.AreEqual(expected, actual);
        }

        /*
        /// <summary>
        ///A test for XPath
        ///</summary>
        [TestMethod()]
        public void XPathTest()
        {
            XElement source = null; // TODO: Initialize to an appropriate value
            XPathString path = null; // TODO: Initialize to an appropriate value
            bool create = false; // TODO: Initialize to an appropriate value
            IEnumerable<XElement> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<XElement> actual;
            actual = XPathExtensions.XPath(source, path, create);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void XGetElementTest()
        {
            XGetElementTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for XGet
        ///</summary>
        public void XGetTestHelper<T>()
        {
            XElement source = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            T @default = default(T); // TODO: Initialize to an appropriate value
            object[] args = null; // TODO: Initialize to an appropriate value
            IEnumerable<T> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<T> actual;
            actual = XPathExtensions.XGet<T>(source, path, @default, args);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void XGetTest()
        {
            XGetTestHelper<GenericParameterHelper>();
        }
         */
    }
}

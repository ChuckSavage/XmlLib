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
        public void Last()
        {
            string path = "pair[last()]";
            object[] args = new object[] { };
            XElement expected = root.Elements("pair").LastOrDefault();
            XElement actual = XPathExtensions.XPathElement(root, path, args);
            Assert.AreEqual(expected.ToString(), actual.ToString());
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

using XmlLib.nXPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XmlLib_Test
{
    /// <summary>
    ///This is a test class for XPath_BracketTest and is intended
    ///to contain all XPath_BracketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XPath_BracketTest
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
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        /// [.='ABC']
        ///</summary>
        [TestMethod()]
        public void Elements_currentNodeEqualsValue()
        {
            XPathString path = new XPathString("[.='Mike']");
            XPath_Bracket target = new XPath_Bracket(path);
            IEnumerable<XElement> elements = root1.Descendants();
            IEnumerable<XElement> expected = root1.Descendants()
                .Where(xe => xe.Value == "Mike");
            IEnumerable<XElement> actual;
            actual = target.Elements(elements);
            CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray());
        }
        /*
        /// <summary>
        ///A test for XPath_Bracket Constructor
        ///</summary>
        [TestMethod()]
        public void XPath_BracketConstructorTest()
        {
            XPathString path = null; // TODO: Initialize to an appropriate value
            XPath_Bracket target = new XPath_Bracket(path);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
         */
    }
}

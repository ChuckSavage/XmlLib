using XmlLib.nXPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XmlLib_Test
{


    /// <summary>
    ///This is a test class for XPathStringTest and is intended
    ///to contain all XPathStringTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XPathStringTest
    {


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
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        public void IsXPath_JustAPath()
        {
            string path = "part/path/nodes";
            object[] values = new object[] { };
            XPathString target = new XPathString(path, values);
            bool actual, expected = false;
            actual = target.IsXPath;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        public void IsXPath_PathWithBrackets()
        {
            string path = "part[key=value]";
            object[] values = null;
            XPathString target = new XPathString(path, values);
            bool actual, expected = true;
            actual = target.IsXPath;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        public void IsXPath_PathWithoutBrackets()
        {
            string path = "//part";
            object[] values = null;
            XPathString target = new XPathString(path, values);
            bool actual, expected = false;
            actual = target.IsXPath;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToPaths
        ///</summary>
        [TestMethod()]
        public void ToPaths_Same()
        {
            string path = "same";
            object[] values = null; 
            XPathString target = new XPathString(path, values);
            XPathString[] expected = new[] { target };
            XPathString[] actual = target.ToPaths(path);
            CollectionAssert.AreEqual(expected, actual);
        }
        /* Split_overrideEquals() and Split_Comparer() check this
        /// <summary>
        ///A test for ToPaths
        ///</summary>
        [TestMethod()]
        public void ToPaths_ToXPathStrings()
        {
            string path = "pair[@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString target = new XPathString(path, values); 
            IEnumerable<string> parts = null; // TODO: Initialize to an appropriate value
            XPathString[] expected = null; // TODO: Initialize to an appropriate value
            XPathString[] actual;
            actual = target.ToPaths(parts);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        */
        /// <summary>
        ///A test for SplitInternal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Git XmlLib vs2008.dll")]
        public void SplitInternalTest()
        {
            string path = "pair[path/to/@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString xp = new XPathString(path, values);

            PrivateObject param0 = new PrivateObject(xp);
            XPathString_Accessor target = new XPathString_Accessor(param0);
            string[] expected = new string[] {
                "pair[path/to/@Key={0}]",
                "Items",
                "Item[Name={1}]",
                "Date"
            };
            string[] actual;
            actual = target.SplitInternal();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SplitInternal
        ///</summary>
        [TestMethod()]
        public void Combine()
        {
            string path = "pair[path/to/@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString xp = new XPathString(path, values);

            string expected = "pair[path/to/@Key={0}]/Items/Item[Name={1}]";
            XPathString actual = XPathString.Combine(xp.PathSegments.Take(3));
            Assert.AreEqual(expected, actual.Format);
            CollectionAssert.AreEqual(values, actual.Values);
        }

        /// <summary>
        ///A test for Split
        ///</summary>
        [TestMethod()]
        public void Split_overrideEquals()
        {
            string path = "pair[path/to/@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString target = new XPathString(path, values);
            XPathString[] expected = new[]{
                new XPathString("pair[path/to/@Key={0}]", 2),
                new XPathString("Items"),
                new XPathString("Item[Name={0}]", "Martin"),
                new XPathString("Date")
            };
            XPathString[] actual;
            actual = target.Split();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Split
        ///</summary>
        [TestMethod()]
        public void Split_Comparer()
        {
            string path = "pair[path/to/@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString target = new XPathString(path, values);
            XPathString[] expected = new[]{
                new XPathString("pair[path/to/@Key={0}]", 2),
                new XPathString("Items"),
                new XPathString("Item[Name={0}]", "Martin"),
                new XPathString("Date")
            };
            XPathString[] actual;
            actual = target.Split();
            CollectionAssert.AreEqual(expected, actual, XPathString.Comparer);
        }
        /*
        /// <summary>
        ///A test for Split
        ///</summary>
        [TestMethod()]
        public void SplitTest1()
        {
            string path = string.Empty; // TODO: Initialize to an appropriate value
            object[] values = null; // TODO: Initialize to an appropriate value
            XPathString target = new XPathString(path, values); // TODO: Initialize to an appropriate value
            string[] separator = null; // TODO: Initialize to an appropriate value
            StringSplitOptions option = new StringSplitOptions(); // TODO: Initialize to an appropriate value
            XPathString[] expected = null; // TODO: Initialize to an appropriate value
            XPathString[] actual;
            actual = target.Split(separator, option);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Split
        ///</summary>
        [TestMethod()]
        public void SplitTest()
        {
            string path = string.Empty; // TODO: Initialize to an appropriate value
            object[] values = null; // TODO: Initialize to an appropriate value
            XPathString target = new XPathString(path, values); // TODO: Initialize to an appropriate value
            char[] separator = null; // TODO: Initialize to an appropriate value
            XPathString[] expected = null; // TODO: Initialize to an appropriate value
            XPathString[] actual;
            actual = target.Split(separator);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        */
        /// <summary>
        ///A test for XPathString Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XPathStringConstructor_EmptyPath()
        {
            string path = string.Empty;
            object[] values = null;
            XPathString target = new XPathString(path, values);
        }

        /// <summary>
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XPathStringConstructor_NullPath()
        {
            string path = null;
            object[] values = null;
            XPathString target = new XPathString(path, values);
        }

        /// <summary>
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        public void XPathStringConstructor_Path_EmbeddedValues()
        {
            string path = "pair[@Key=2]/Items/Item[Name='Martin']/Date";
            object[] values = null;
            XPathString target = new XPathString(path, values);
            string actual, expected = "pair[@Key=2]/Items/Item[Name='Martin']/Date";
            actual = target.Text;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsXPath
        ///</summary>
        [TestMethod()]
        public void XPathStringConstructor_Path_ExternalValues()
        {
            string path = "pair[@Key={0}]/Items/Item[Name={1}]/Date";
            object[] values = new object[] { 2, "Martin" };
            XPathString target = new XPathString(path, values);
            string actual, expected = "pair[@Key=2]/Items/Item[Name=Martin]/Date";
            actual = target.Text;
            Assert.AreEqual(expected, actual);
        }

    }
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib.nXPath;

namespace XmlLib_Test
{
    [TestClass]
    public class XPath_PartTest
    {
        /// <summary>
        ///A test for XPath_Part
        ///</summary>
        [TestMethod()]
        public void XPath_Part_KVP_EqualL()
        {
            XPathString path = new XPathString("[@to = {0}]", 0);
            XPath_Bracket target = new XPath_Bracket(path);
            Assert.AreEqual(target.Parts.Length, 1);
            var part = target.Parts[0]; // XPath_Part
            Assert.AreEqual(part.ElementAt, false);
            Assert.AreEqual(part.IsValueAttribute, true);
            Assert.AreEqual(part.KVP, true);
            Assert.AreEqual(null, part.Function);
            Assert.AreEqual(part.Equal, true);
            Assert.AreEqual(part.GreaterThan, false);
            Assert.AreEqual(part.GreaterThanOrEqual, false);
            Assert.AreEqual(part.LessThan, false);
            Assert.AreEqual(part.LessThanOrEqual, false);
            Assert.AreEqual(part.NotEqual, false);
            Assert.AreEqual(part.Key, "to");
            Assert.AreEqual(part.Value, 0);
        }
        /// <summary>
        ///A test for XPath_Part
        ///</summary>
        [TestMethod()]
        public void XPath_Part_KVP_EqualR()
        {
            XPathString path = new XPathString("[{0}=to]", 0);
            XPath_Bracket target = new XPath_Bracket(path);
            Assert.AreEqual(target.Parts.Length, 1);
            var part = target.Parts[0]; // XPath_Part
            Assert.AreEqual(part.ElementAt, false);
            Assert.AreEqual(part.IsValueAttribute, false);
            Assert.AreEqual(part.KVP, true);
            Assert.AreEqual(null, part.Function);
            Assert.AreEqual(part.Equal, true);
            Assert.AreEqual(part.GreaterThan, false);
            Assert.AreEqual(part.GreaterThanOrEqual, false);
            Assert.AreEqual(part.LessThan, false);
            Assert.AreEqual(part.LessThanOrEqual, false);
            Assert.AreEqual(part.NotEqual, false);
            Assert.AreEqual(part.Key, "to");
            Assert.AreEqual(part.Value, 0);
        }
        /// <summary>
        ///A test for XPath_Part
        ///</summary>
        [TestMethod()]
        public void XPath_Part_KVP_Left()
        {
            XPathString path = new XPathString("[@to < {0}]", 0);
            XPath_Bracket target = new XPath_Bracket(path);
            Assert.AreEqual(target.Parts.Length, 1);
            var part = target.Parts[0]; // XPath_Part
            Assert.AreEqual(part.ElementAt, false);
            Assert.AreEqual(part.Equal, false);
            Assert.AreEqual(null, part.Function);
            Assert.AreEqual(part.GreaterThan, false);
            Assert.AreEqual(part.GreaterThanOrEqual, false);
            Assert.AreEqual(part.IsValueAttribute, true);
            Assert.AreEqual(part.Key, "to");
            Assert.AreEqual(part.KVP, true);
            Assert.AreEqual(part.LessThan, true);
            Assert.AreEqual(part.LessThanOrEqual, false);
            Assert.AreEqual(part.NotEqual, false);
            Assert.AreEqual(part.Value, 0);
        }

        /// <summary>
        ///A test for XPath_Part
        ///</summary>
        [TestMethod()]
        public void XPath_Part_KVP_Right()
        {
            /*
             * Swaps the expression to be [@to >= {0}]
             * So we compare for > and >=
             */
            XPathString path = new XPathString("[{0} < @to]", 0);
            XPath_Bracket target = new XPath_Bracket(path);
            Assert.AreEqual(target.Parts.Length, 1);
            var part = target.Parts[0]; // XPath_Part
            Assert.AreEqual(part.ElementAt, false);
            Assert.AreEqual(null, part.Function);
            Assert.AreEqual(part.IsValueAttribute, true);
            Assert.AreEqual(part.KVP, true);
            Assert.AreEqual(part.Equal, true);
            Assert.AreEqual(part.GreaterThan, true);
            Assert.AreEqual(part.GreaterThanOrEqual, true);
            Assert.AreEqual(part.LessThan, false);
            Assert.AreEqual(part.LessThanOrEqual, false);
            Assert.AreEqual(part.NotEqual, false);
            Assert.AreEqual(part.Key, "to");
            Assert.AreEqual(part.Value, 0);
        }

    }
}

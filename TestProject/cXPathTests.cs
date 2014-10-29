using System;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib.nXPath;
using XmlLib;
using System.Linq;

namespace XmlLib_Test
{
    [TestClass]
    public class cXPathTests
    {
        // public static IEnumerable<XElement> Enumerable(XElement source, XPathString path, bool create)
        [TestMethod]
        public void Enumerable_CreateNode_RootUnchanged()
        {
            XElement root = new XElement("root");
            XElement expected = new XElement("root");
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            var actual = cXPath.Enumerable(root, xpath, true);
            Assert.AreEqual(root.ToString(), expected.ToString()); // root remains unchanged
        }
        [TestMethod]
        public void Enumerable_Create1_SiblingAdd() // Root changed
        {
            XElement root = new XElement("root", new XElement("NodeA", new XElement("Id", "a")));
            XElement notexpected = new XElement("root");
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            var actual = cXPath.Enumerable(root, xpath, true);
            Assert.AreNotEqual(root.ToString(), notexpected.ToString()); // root was changed
        }
        [TestMethod]
        public void Enumerable_Create1_SiblingAdd2() // Root changed
        {
            // Add not-found NodeA as a sibling to another NodeA
            XElement root = new XElement("root", new XElement("NodeA", new XElement("Id", "a")));
            XElement expected = new XElement("root", new XElement("NodeA", new XElement("Id", "a")));
            expected.Add(new XElement("NodeA",
                new XElement("Id", "A"),
                new XElement("ChildA", new XAttribute("key", "1"))
                ));
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            var actual = cXPath.Enumerable(root, xpath, true);
            Assert.AreEqual(root.ToString(), expected.ToString()); // root was changed
        }
        [TestMethod]
        public void Enumerable_Create1_ChildAdd1() // Root changed
        {
            // Add not-found key element to existing NodeA element
            XElement root = new XElement("root", new XElement("NodeA", new XElement("Id", "A")));
            XElement expected = new XElement("root", 
                new XElement("NodeA", 
                    new XElement("Id", "A"),
                    new XElement("ChildA", new XAttribute("key", "1"))
                )
            );
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            var actual = cXPath.Enumerable(root, xpath, true);
            Assert.AreEqual(root.ToString(), expected.ToString()); // root was changed
        }

        [TestMethod]
        public void Enumerable_CreateNode_RootUnchanged1()
        {
            XElement root = new XElement("root");
            XElement expected = new XElement("ChildA", new XAttribute("key", "1"));
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            XElement actual = cXPath.Enumerable(root, xpath, true).First();
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }
        [TestMethod]
        public void Enumerable_CreateNode_RootUnchanged2()
        {
            XElement root = new XElement("root");
            XElement expected = new XElement("NodeA",
                new XElement("Id", "A"),
                new XElement("ChildA", new XAttribute("key", "1"))
                );
            XPathString xpath = new XPathString("//NodeA[Id={0}]/ChildA[@key={1}]", "A", "1");
            XElement actual = cXPath.Enumerable(root, xpath, true).First().Root();
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }
    }
}

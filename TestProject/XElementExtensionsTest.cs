using System;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib;

namespace XmlLib_Test
{
    [TestClass]
    public class XElementExtensionsTest
    {
        #region Get
        #region Get_FromElement

        class Get_FromElement_Class
        {
            public readonly string Value;

            public Get_FromElement_Class(XElement source)
            {
                Value = source.Value;
            }
        }

        [TestMethod]
        public void Get_FromElement()
        {
            string value = "A value to test";
            XElement test = new XElement("Node", value);
            Get_FromElement_Class result = test.Get<Get_FromElement_Class>("", null);
            Assert.AreEqual(string.Compare(value, result.Value), 0);
        }
        #endregion
        #region Get_FromString

        class Get_FromString_Class
        {
            public readonly string Value;

            public Get_FromString_Class(string source)
            {
                Value = source;
            }
        }

        [TestMethod]
        public void Get_FromString()
        {
            string value = "A value to test";
            XElement test = new XElement("Node", value);
            Get_FromString_Class result = test.Get<Get_FromString_Class>("", null);
            Assert.AreEqual(string.Compare(value, result.Value), 0);
        }
        #endregion

        #region Get_FromTryParse

        class Get_FromTryParse_Class
        {
            public string Value { get; private set; }
            public static bool TryParse(string s, out Get_FromTryParse_Class result)
            {
                result = new Get_FromTryParse_Class
                {
                    Value = s
                };
                return true;
            }
        }

        [TestMethod]
        public void Get_FromTryParse()
        {
            string value = "A value to test";
            XElement test = new XElement("Node", value);
            Get_FromTryParse_Class result = test.Get<Get_FromTryParse_Class>("", null);
            Assert.AreEqual(string.Compare(value, result.Value), 0);
        }
        #endregion

        [TestMethod]
        public void Get_StringValue()
        {
            string value = "A value to test";
            XElement test = new XElement("Node", value);
            string result = test.Get("", "");
            Assert.AreEqual(string.Compare(value, result), 0);
        }

        [TestMethod]
        public void Get_IntValue()
        {
            int value = 100;
            XElement test = new XElement("Node", value);
            int result = test.Get("", 0);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Get_DateTimeValue()
        {
            DateTime value = DateTime.Now;
            XElement test = new XElement("Node", value);
            DateTime result = test.Get("", DateTime.MinValue);
            Assert.AreEqual(value.Ticks, result.Ticks);
        }

        [TestMethod]
        public void Get_StringAttribute()
        {
            string value = "A value to test";
            XElement test = new XElement("Node", new XAttribute("A", value));
            string result = test.Get("A", "");
            Assert.AreEqual(string.Compare(value, result), 0);
        }

        [TestMethod]
        public void Get_IntAttribute()
        {
            int value = 100;
            XElement test = new XElement("Node", new XAttribute("A", value));
            int result = test.Get("A", 0);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Get_DateTimeAttribute()
        {
            DateTime value = DateTime.Now;
            XElement test = new XElement("Node", new XAttribute("A", value));
            DateTime result = test.Get("A", DateTime.MinValue);
            Assert.AreEqual(value.Ticks, result.Ticks);
        }
        #endregion
    }
}

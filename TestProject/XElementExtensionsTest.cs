﻿using System;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib;

namespace XmlLib_Test
{
    [TestClass]
    public class XElementExtensionsTest
    {
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

    }
}

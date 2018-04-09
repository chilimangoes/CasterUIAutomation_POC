using CasterUIAutomation.Communication;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace CasterUIAutomation.Tests
{
    [TestClass]
    public class HttpUtilityTests
    {
        [TestMethod]
        public void HttpUtility_TestQueryStringParsing()
        {
            var parametersIn = new Dictionary<string, string>()
                {
                    { "prop1", "hello" },
                    { "prop2", "world" },
                    { "prop&name", "<propval>" },
                    { "prop with space", " val space at begin/end " },
                    { "prop spcace at end ", " val special chars . / ? \\ \" & + % # @ :" },
                    { "", "blank prop" },
                    { "blank val", "" },
                    { "multi line", "line 1\r\nline 2" },
                    { "null value", null }
                };

            StringBuilder builder = new StringBuilder();
            foreach (var pair in parametersIn)
            {
                if (builder.Length >0)
                {
                    builder.Append('&');
                }
                builder.Append(WebUtility.UrlEncode(pair.Key));
                if (pair.Value != null)
                {
                    builder.Append('=');
                    builder.Append(WebUtility.UrlEncode(pair.Value));
                }
            }
            builder.Append("&"); // test what happens if there's a null parameter

            var parametersOut = HttpUtility.ParseQueryStringParameters(builder.ToString());
            Assert.IsNotNull(parametersOut, "parametersOut is null");
            Assert.AreEqual(parametersIn.Count, parametersOut.Count, "Parameter count does not match.");
            foreach (string key in parametersIn.Keys)
            {
                Assert.IsTrue(parametersOut.ContainsKey(key), "parametersOut is missing key: " + key);
                Assert.AreEqual(parametersIn[key], parametersOut[key]);
            }
        }
    }
}

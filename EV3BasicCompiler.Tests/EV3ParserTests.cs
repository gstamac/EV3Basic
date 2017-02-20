using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class EV3ParserTests
    {
        [TestMethod]
        public void ShouldDeclareInt()
        {
            TestParse(@"
                i = 10
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenValueIsNegative()
        {
            TestParse(@"
                i = -10
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat()
        {
            TestParse(@"
                i = 10.3
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenValueIsNegative()
        {
            TestParse(@"
                i = -10.3
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareString()
        {
            TestParse(@"
                i = ""X""
            ", @"
                DATAS VI 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareIntArray()
        {
            TestParse(@"
                i[6] = 2
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray()
        {
            TestParse(@"
                i[6] = 2.3
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareStringArray()
        {
            TestParse(@"
                i[7] = ""X""
            ", @"
                ARRAY16 VI 7
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithReference()
        {
            TestParse(@"
                i = 10.3
                j = i
            ", @"
                DATAF VI
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithFormula()
        {
            TestParse(@"
                i = 10.3
                j = i + i
            ", @"
                DATAF VI
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithReference()
        {
            TestParse(@"
                i = ""X""
                j = i
            ", @"
                DATAS VI 252
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithFormula()
        {
            TestParse(@"
                i = ""X""
                j = i + i
            ", @"
                DATAS VI 252
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoop()
        {
            TestParse(@"
                For i = 10 To 24
                EndFor
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoopWithStep()
        {
            TestParse(@"
                For i = 10 To 24 Step 2
                EndFor
            ", @"
                DATAF VI
            ");
        }

        private void TestParse(string sbCode, string ev3Code)
        {
            EV3Parser parser = new EV3Parser();
            parser.Parse(new StringReader(sbCode));
            Console.WriteLine(parser.Dump());

            StringWriter writer = new StringWriter();

            parser.GenerateEV3Code(writer);

            CleanupCode(writer.ToString()).Should().Be(CleanupCode(ev3Code));
        }

        private string CleanupCode(string ev3Code)
        {
            string outEv3Code = new Regex("//[^\n\r]*").Replace(ev3Code, "");
            outEv3Code = new Regex("[ ]+\r").Replace(outEv3Code, "\r");
            outEv3Code = new Regex("\n[ ]+").Replace(outEv3Code, "\n");
            outEv3Code = new Regex("(\r\n)+").Replace(outEv3Code, "\r\n");
            return outEv3Code.Trim('\r', '\n');
        }
    }
}

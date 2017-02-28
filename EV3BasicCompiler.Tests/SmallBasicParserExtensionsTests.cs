using FluentAssertions;
using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class SmallBasicParserExtensionsTests
    {
        [TestMethod]
        public void Test()
        {
            TestIt(@"
                i = 10
                Method()
                Obj.Method()
            ", "Assignment", "SubroutineCall", "MethodCall");
        }

        [TestMethod]
        public void TestFor()
        {
            TestIt(@"
                i = 10
                For j = 1 To 10 
                    Method()
                EndFor
            ", "Assignment", "For", "SubroutineCall");
        }

        [TestMethod]
        public void TestWhile()
        {
            TestIt(@"
                i = 10
                While (1 = 1)
                    Method()
                EndWhile
            ", "Assignment", "While", "SubroutineCall");
        }

        [TestMethod]
        public void TestIfThenElse()
        {
            TestIt(@"
                i = 10
                If (1 == 1) Then
                    Method()
                Else
                    Obj.Method()
                EndIf
            ", "Assignment", "If", "SubroutineCall", "MethodCall");
        }

        [TestMethod]
        public void TestIfThenElseIf()
        {
            TestIt(@"
                i = 10
                If (1 == 1) Then
                    Method()
                ElseIf (2 == 2)
                    Obj.Method()
                Else
                    Method()
                EndIf
            ", "Assignment", "If", "SubroutineCall", "ElseIf", "MethodCall", "SubroutineCall");
        }

        [TestMethod]
        public void TestSub()
        {
            TestIt(@"
                i = 10
                Sub Func
                    Method()
                EndSub
            ", "Assignment", "Subroutine", "SubroutineCall");
        }

        private void TestIt(string sbCode, params string[] statements)
        {
            Parser parser = new Parser();
            using (StringReader stringReader = new StringReader(sbCode))
            {
                parser.Parse(stringReader);
                string[] statementList = SmallBasicParserExtensions.GetStatements(parser).Select(s => s.GetType().Name.Replace("Statement", "")).Where(s => !s.Equals("Empty")).ToArray();
                foreach (string item in statementList)
                    Console.WriteLine(item);
                Console.WriteLine(parser.Dump());

                statementList.ShouldAllBeEquivalentTo(statements);
            }
        }
    }
}

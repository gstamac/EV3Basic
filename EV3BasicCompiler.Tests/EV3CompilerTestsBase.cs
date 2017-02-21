using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    public class EV3CompilerTestsBase
    {
        protected void TestIt(string sbCode, string ev3Code)
        {
            EV3Compiler compiler = new EV3Compiler();
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Parse(stringReader);
                Console.WriteLine(compiler.Dump());

                compiler.GenerateEV3Code(writer);
                string code = writer.ToString();

                Console.WriteLine("======> ERRORS <======");
                foreach (Error error in compiler.Errors)
                    Console.WriteLine(error);

                Console.WriteLine("======> CODE <======");
                Console.WriteLine(code);
                Console.WriteLine("======> END CODE <======");

                CleanupCode(CleanupCompiledCode(code)).Should().Be(CleanupCode(ev3Code));
                compiler.Errors.Should().BeEmpty();
            }
        }

        protected void TestItOld(string sbCode, string ev3Code)
        {
            using (MemoryStream fs = new MemoryStream(Encoding.ASCII.GetBytes(sbCode)))
            using (MemoryStream ofs = new MemoryStream())
            {
                List<String> errors = new List<String>();

                EV3BasicCompiler.Compiler c = new EV3BasicCompiler.Compiler();
                c.Compile(fs, ofs, errors);
                string code = Encoding.ASCII.GetString(ofs.ToArray());

                Console.WriteLine("======> ERRORS <======");
                foreach (string error in errors)
                    Console.WriteLine(error);

                Console.WriteLine("======> CODE <======");
                Console.WriteLine(code);
                Console.WriteLine("======> END CODE <======");

                CleanupCode(CleanupCompiledCode(code)).Should().Be(CleanupCode(ev3Code));
                errors.Should().BeEmpty();
            }
        }

        protected void TestParseFailure(string sbCode, string message, int line, int column)
        {
            EV3Compiler compiler = new EV3Compiler();
            using (StringReader stringReader = new StringReader(sbCode))
            {
                compiler.Parse(stringReader);
                Console.WriteLine(compiler.Dump());

                compiler.Errors.ShouldAllBeEquivalentTo(new Error(message, line, column));
            }
        }

        protected virtual string CleanupCompiledCode(string ev3Code)
        {
            return ev3Code;
        }

        protected string CleanupCode(string ev3Code)
        {
            ev3Code = new Regex("//[^\n\r]*").Replace(ev3Code, "");
            ev3Code = new Regex("[ \r\n\t]*[\r\n]+[ \r\n\t]*").Replace(ev3Code, "\r\n");

            return ev3Code.Trim('\r', '\n');
        }

        protected string RemoveLines(string ev3Code, params string[] lines)
        {
            foreach (string line in lines)
            {
                ev3Code = new Regex($"[ \t]*{line}[ \r\n]+").Replace(ev3Code, "");
            }
            return ev3Code;
        }

    }
}

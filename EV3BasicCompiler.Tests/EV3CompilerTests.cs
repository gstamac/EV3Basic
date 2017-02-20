using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    public class EV3CompilerTests
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

                CleanupCode(CleanupCompiledCode(writer.ToString())).Should().Be(CleanupCode(ev3Code));
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

                CleanupCode(CleanupCompiledCode(Encoding.ASCII.GetString(ofs.ToArray()))).Should().Be(CleanupCode(ev3Code));
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
            //ev3Code = new Regex("[ ]+\r").Replace(ev3Code, "\r");
            //ev3Code = new Regex("\n[ ]+").Replace(ev3Code, "\n");
            ev3Code = new Regex("[ \r\n]*[\r\n]+[ \r\n]*").Replace(ev3Code, "\r\n");

            //outEv3Code = RemoveLines(outEv3Code,
            //    "DATA16 FD_NATIVECODECOMMAND",
            //    "DATA16 FD_NATIVECODERESPONSE",
            //    "DATA32 STOPLCDUPDATE",
            //    "DATA32 NUMMAILBOXES",
            //    "ARRAY16 LOCKS 2");

            //outEv3Code = RemoveLines(outEv3Code,
            //    "MOVE32_32 0 STOPLCDUPDATE",
            //    "MOVE32_32 0 NUMMAILBOXES",
            //    "OUTPUT_RESET 0 15",
            //    "INPUT_DEVICE CLR_ALL -1",
            //    "ARRAY CREATE8 0 LOCKS",
            //    "ARRAY CREATE8 1 LOCKS",
            //    "CALL PROGRAM_MAIN -1",
            //    "PROGRAM_STOP -1");

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

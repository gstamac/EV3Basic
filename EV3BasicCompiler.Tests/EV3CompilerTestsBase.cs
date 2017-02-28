using FluentAssertions;
using Microsoft.SmallBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    public class EV3CompilerTestsBase
    {
        protected void TestInitialization(string sbCode, string expectedCode)
        {
            TestIt(sbCode, expectedCode, ExtractInitializationCode);
        }

        protected void TestDeclaration(string sbCode, string expectedCode)
        {
            TestIt(sbCode, expectedCode, ExtractDeclarationCode);
        }

        protected void TestInitializationOld(string sbCode, string expectedCode)
        {
            TestItOld(sbCode, expectedCode, ExtractInitializationCode);
        }

        protected void TestDeclarationOld(string sbCode, string expectedCode)
        {
            TestItOld(sbCode, expectedCode, ExtractDeclarationCode);
        }

        protected void TestFull(string sbCode, string expectedCode)
        {
            TestIt(sbCode, expectedCode, c => c);
        }

        protected void TestFullOld(string sbCode, string expectedCode)
        {
            TestItOld(sbCode, expectedCode, c => c);
        }

        protected void TestIt(string sbCode, string expectedCode, Func<string, string> extractCodeFunc)
        {
            using (EV3Compiler compiler = new EV3Compiler())
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Compile(stringReader, writer);

                string code = writer.ToString();

                Console.WriteLine("======> ERRORS <======");
                foreach (Error error in compiler.Errors)
                    Console.WriteLine(error);

                string extractedCode = extractCodeFunc(code);

                Console.WriteLine("======> EXTRACTED/EXPECTED CODE <======");
                DumpCodeSideBySide(extractedCode, expectedCode);
                Console.WriteLine("======> FULL CODE <======");
                Console.WriteLine(code);
                Console.WriteLine("======> END CODE <======");

                Console.WriteLine(compiler.Dump());

                CleanupCode(extractedCode).Should().Be(CleanupCode(expectedCode));
                compiler.Errors.Should().BeEmpty();
            }
        }

        protected void TestItDump(string sbCode, string expectedCode, Func<string, string> extractCodeFunc = null)
        {
            Parser parser = new Parser();
            using (StringReader stringReader = new StringReader(sbCode))
            {
                parser.Parse(stringReader);
                try
                {
                    parser.AttachCompilers(null);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ATTACH ERROR: " + e);
                    Console.WriteLine(e.StackTrace);
                }
                Console.WriteLine(parser.Dump());
            }

            true.Should().BeFalse();
        }

        protected void TestItOld(string sbCode, string expectedCode, Func<string, string> extractCodeFunc)
        {
            sbCode = Regex.Replace(sbCode, @"[ \r\n\t]*'PRAGMA", "'PRAGMA");
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

                string extractedCode = extractCodeFunc(code);

                Console.WriteLine("======> EXTRACTED/EXPECTED CODE <======");
                DumpCodeSideBySide(extractedCode, expectedCode);
                Console.WriteLine("======> FULL CODE <======");
                Console.WriteLine(code);
                Console.WriteLine("======> END CODE <======");

                CleanupCode(extractedCode).Should().Be(CleanupCode(expectedCode));
                errors.Should().BeEmpty();
            }
        }

        private void DumpCodeSideBySide(string compiledCode, string expectedCode)
        {
            string[] compiledLines = Regex.Split(compiledCode, "[\n\r\t ]*[\n\r]+[\n\r\t ]*").Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            string[] expectedLines = Regex.Split(expectedCode, "[\n\r\t ]*[\n\r]+[\n\r\t ]*").Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            int minLength = Math.Min(compiledLines.Length, expectedLines.Length);
            int maxWidth = compiledLines.Any() ? compiledLines.Max(l => l.Length) + 4 : 30;
            Console.WriteLine();
            for (int i = 0; i < minLength; i++)
            {
                Console.WriteLine(compiledLines[i].PadRight(maxWidth) + expectedLines[i]);
            }
            for (int i = minLength; i < compiledLines.Length; i++)
            {
                Console.WriteLine(compiledLines[i].PadRight(maxWidth));
            }
            for (int i = minLength; i < expectedLines.Length; i++)
            {
                Console.WriteLine("".PadRight(maxWidth) + expectedLines[i]);
            }
            Console.WriteLine();
        }

        protected void TestCompileFailure(string sbCode, string message, int line, int column)
        {
            using (EV3Compiler compiler = new EV3Compiler())
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Compile(stringReader, writer);

                Console.WriteLine(compiler.Dump());

                compiler.Errors.Should().Contain(e => e.Message == message && e.Line == line && e.Column == column);
            }
        }

        protected string CleanupCode(string ev3Code)
        {
            ev3Code = new Regex(@"//[^\n\r]*").Replace(ev3Code, "");
            ev3Code = new Regex(@"[ \r\n\t]*[\r\n]+[ \r\n\t]*").Replace(ev3Code, "\r\n");

            return ev3Code.Trim('\r', '\n');
        }

        protected string ExtractDeclarationCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, @"(.*)vmthread MAIN.*", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            match = Regex.Match(ev3Code, @".*ARRAY16 LOCKS 2[^\n\r]*(.*)", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return ev3Code;
        }

        protected string ExtractInitializationCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, @"vmthread MAIN[^{]*{([^}]*)}", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            match = Regex.Match(ev3Code, @"ARRAY CREATE8 0 LOCKS([^}]*)ARRAY CREATE8 1 LOCKS", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return ev3Code;
        }

        protected string ExtractFullMainProgramCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, @"(subcall PROGRAM_MAIN[^{]*{[^}]*})", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return Regex.Replace(ev3Code, @"[\n\r]+[ \t]*SUB_[^\:]+:[^}]*ENDSUB_[^\:]+:", "");
        }

        protected string ExtractMainProgramCode(string ev3Code)
        {
            ev3Code = ExtractFullMainProgramCode(ev3Code);

            Match match = Regex.Match(ev3Code, @"MOVE8_8 0 STACKPOINTER([^}]*)ENDTHREAD:", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return ev3Code;
        }

        protected string ExtractSubroutinesCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, @"subcall PROGRAM_MAIN[^{]*{[^}]*ENDTHREAD:[^}:]*(SUB_[^\:]+:[^}]*ENDSUB_[^\:]+:)[^}]*}", RegexOptions.Singleline);
            return match.Groups[1].ToString();
        }

        protected string RemoveLines(string ev3Code, params string[] lines)
        {
            foreach (string line in lines)
            {
                ev3Code = new Regex($@"[ \t]*{line}[ \r\n]+").Replace(ev3Code, "");
            }
            return ev3Code;
        }

    }
}

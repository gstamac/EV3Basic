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
        protected void TestInitialization(string sbCode, string expectedCode, bool doOptimization = false)
        {
            TestIt(sbCode, expectedCode, ExtractInitializationCode, doOptimization);
        }

        protected void TestDeclaration(string sbCode, string expectedCode, bool doOptimization = false)
        {
            TestIt(sbCode, expectedCode, ExtractDeclarationCode, doOptimization);
        }

        protected void TestInitializationOld(string sbCode, string expectedCode, bool doOptimization = false)
        {
            TestItOld(sbCode, expectedCode, ExtractInitializationCode);
        }

        protected void TestDeclarationOld(string sbCode, string expectedCode, bool doOptimization = false)
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

        protected void TestIt(string sbCode, string expectedCode, Func<string, string> extractCodeFunc, bool doOptimization = false)
        {
            if (!doOptimization)
                sbCode = "'PRAGMA NOOPTIMIZATION" + Environment.NewLine + sbCode;

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
                if (!string.IsNullOrWhiteSpace(expectedCode))
                {
                    extractedCode = NormalizeReferences(NormalizeLabels(extractedCode));
                    expectedCode = NormalizeReferences(NormalizeLabels(expectedCode));
                }

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

        protected void TestItDump(string sbCode, string expectedCode, Func<string, string> extractCodeFunc = null, bool doOptimization = false)
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

        protected void TestItOld(string sbCode, string expectedCode, Func<string, string> extractCodeFunc, bool doOptimization = false)
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
                if (!string.IsNullOrWhiteSpace(expectedCode))
                {
                    extractedCode = NormalizeReferences(NormalizeLabels(extractedCode));
                    expectedCode = NormalizeReferences(NormalizeLabels(expectedCode));
                }

                Console.WriteLine("======> EXTRACTED/EXPECTED CODE <======");
                DumpCodeSideBySide(extractedCode, expectedCode);
                Console.WriteLine("======> FULL CODE <======");
                Console.WriteLine(code);
                Console.WriteLine("======> END CODE <======");

                CleanupCode(extractedCode).Should().Be(CleanupCode(expectedCode));
                errors.Should().BeEmpty();
            }
        }

        protected void DumpCodeSideBySide(string compiledCode, string expectedCode)
        {
            List<string> compiledLines = Regex.Split(compiledCode, "[\t ]*[\n\r]+").Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Replace("\t", "    ")).ToList();
            List<string> expectedLines = Regex.Split(expectedCode, "[\t ]*[\n\r]+").Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            if (expectedLines.Count == 0)
            {
                for (int i = 0; i < compiledLines.Count; i++)
                {
                    Console.WriteLine("".PadRight(12) + compiledLines[i]);
                }
                return;    
            }


            int maxWidth = compiledLines.Max(l => l.Length) + 3;
            Console.WriteLine();
            while (compiledLines.Count > 0 || expectedLines.Count > 0)
            {
                if (compiledLines.Count == 0)
                {
                    foreach (string expectedLine in expectedLines)
                        Console.WriteLine("! " + "".PadRight(maxWidth) + expectedLine);
                    break;
                }
                else if (expectedLines.Count == 0)
                {
                    foreach (string compiledLine in compiledLines)
                        Console.WriteLine("! " + compiledLine);
                    break;
                }
                else if (!RemoveCommentAndTrim(compiledLines[0]).Equals(RemoveCommentAndTrim(expectedLines[0])))
                {
                    var firstEqual = compiledLines.Select((l, i) => new { Line = l, Index = i })
                        .Join(expectedLines.Select((l, i) => new { Line = l, Index = i }),
                            c => RemoveCommentAndTrim(c.Line),
                            e => RemoveCommentAndTrim(e.Line),
                            (c, e) => new { CompiledIndex = c.Index, ExpectedIndex = e.Index })
                        .OrderBy(x => x.CompiledIndex + x.ExpectedIndex)
                        .FirstOrDefault();
                    int compiledIndex = compiledLines.Count;
                    int expectedIndex = expectedLines.Count;
                    if (firstEqual != null)
                    {
                        compiledIndex = firstEqual.CompiledIndex;
                        expectedIndex = firstEqual.ExpectedIndex;
                    }
                    int minIndex = Math.Min(compiledIndex, expectedIndex);
                    for (int i = 0; i < minIndex; i++)
                    {
                        Console.WriteLine("! " + compiledLines[i].PadRight(maxWidth) + expectedLines[i]);
                    }
                    for (int i = minIndex; i < compiledIndex; i++)
                        Console.WriteLine("! " + compiledLines[i]);
                    for (int i = minIndex; i < expectedIndex; i++)
                        Console.WriteLine("! " + "".PadRight(maxWidth) + expectedLines[i]);
                    compiledLines.RemoveRange(0, compiledIndex);
                    expectedLines.RemoveRange(0, expectedIndex);
                }
                else
                {
                    Console.WriteLine("  " + compiledLines[0].PadRight(maxWidth) + expectedLines[0]);
                    expectedLines.RemoveAt(0);
                    compiledLines.RemoveAt(0);
                }
            }
            //compiledLines = Regex.Split(compiledCode, "[\t ]*[\n\r]+").Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Replace("\t", "    ")).ToList();
            //expectedLines = Regex.Split(expectedCode, "[\t ]*[\n\r]+").Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            //int minLength = Math.Min(compiledLines.Count, expectedLines.Count);
            //for (int i = 0; i < minLength; i++)
            //{
            //    Console.WriteLine(compiledLines[i].PadRight(maxWidth) + expectedLines[i]);
            //}
            //for (int i = minLength; i < compiledLines.Count; i++)
            //{
            //    Console.WriteLine(compiledLines[i]);
            //}
            //for (int i = minLength; i < expectedLines.Count; i++)
            //{
            //    Console.WriteLine("".PadRight(maxWidth) + expectedLines[i]);
            //}
            Console.WriteLine();
        }

        protected static string RemoveCommentAndTrim(string line)
        {
            return Regex.Replace(line, "//.*", "").Trim();
        }

        protected void TestCompileFailure(string sbCode, string message, int line, int column, bool doOptimization = false)
        {
            if (!doOptimization)
                sbCode = "'PRAGMA NOOPTIMIZATION" + Environment.NewLine + sbCode;

            using (EV3Compiler compiler = new EV3Compiler())
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Compile(stringReader, writer);

                Console.WriteLine(compiler.Dump());

                compiler.Errors.Should().Contain(e => e.Message == message && e.Line == line && e.Column == column);
            }
        }

        protected void TestCompileFailure(string sbCode, string message, bool doOptimization = false)
        {
            if (!doOptimization)
                sbCode = "'PRAGMA NOOPTIMIZATION" + Environment.NewLine + sbCode;

            using (EV3Compiler compiler = new EV3Compiler())
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Compile(stringReader, writer);

                Console.WriteLine(compiler.Dump());

                compiler.Errors.Should().Contain(e => e.Message == message);
            }
        }

        protected void TestCompileFailure(string sbCode, bool doOptimization = false)
        {
            if (!doOptimization)
                sbCode = "'PRAGMA NOOPTIMIZATION" + Environment.NewLine + sbCode;

            using (EV3Compiler compiler = new EV3Compiler())
            using (StringReader stringReader = new StringReader(sbCode))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Compile(stringReader, writer);

                Console.WriteLine(compiler.Dump());

                compiler.Errors.Should().NotBeEmpty();
            }
        }

        protected string CleanupCode(string ev3Code)
        {
            ev3Code = new Regex(@"//[^\n\r]*").Replace(ev3Code, "");
            ev3Code = new Regex(@"[ \r\n\t]*[\r\n]+[ \r\n\t]*").Replace(ev3Code, "\r\n");

            return ev3Code.Trim('\r', '\n');
        }

        protected string NormalizeLabels(string code)
        {
            var labels = Regex.Matches(code, $@"^[ \r\n\t]*((for|while|endif|CALLSUB|dispatch|alreadylaunched|motorwaiting|motornotbusy)([0-9]+)):[ \t]*\r?$", RegexOptions.Multiline).OfType<Match>()
                .Select(m => new { Label = m.Groups[2].Value, Id = m.Groups[3].Value })
                .ToArray();
            for (int i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                if (label.Label.Equals("for"))
                {
                    code = Regex.Replace(code, $@"\b(for|forbody|endfor){label.Id}\b", m => $"DUMMY___{m.Groups[1].Value}{i}", RegexOptions.Singleline);
                }
                else if (label.Label.Equals("while"))
                {
                    code = Regex.Replace(code, $@"\b(while|whilebody|endwhile){label.Id}\b", m => $"DUMMY___{m.Groups[1].Value}{i}", RegexOptions.Singleline);
                }
                else if (label.Label.Equals("endif"))
                {
                    code = Regex.Replace(code, $@"\bendif{label.Id}\b", $"DUMMY___endif{i}", RegexOptions.Singleline);
                    code = Regex.Replace(code, $@"\belse{label.Id}(_[0-9]+)\b", m => $"DUMMY___else{i}{m.Groups[1].Value}", RegexOptions.Singleline);
                }
                else
                {
                    code = Regex.Replace(code, $@"\b{label.Label}{label.Id}\b", $"DUMMY___{label.Label}{i}", RegexOptions.Singleline);
                }
            }

            return code.Replace("DUMMY___", "");

            //string[] labels = Regex.Matches(code, $@"^[ \r\n\t]*([^\n\r\t ]+):[ \t]*\r?$", RegexOptions.Multiline).OfType<Match>()
            //    .Select(m => m.Groups[1].Value)
            //    .Where(l => !l.Equals("ENDTHREAD") && !l.StartsWith("SUB_") && !l.StartsWith("ENDSUB_"))
            //    .ToArray();
            //int callsubId = 0;
            //for (int i = 0; i < labels.Length; i++)
            //{
            //    if (labels[i].StartsWith("CALLSUB"))
            //    {
            //        string newLabel = $"XCALLSUB{callsubId}";
            //        code = Regex.Replace(code, $@"\b{labels[i]}\b", newLabel, RegexOptions.Singleline);
            //        callsubId++;
            //    }
            //    else
            //    {
            //        string newLabel = $"dummylabel{i}";
            //        code = Regex.Replace(code, $@"^([ \t]*){labels[i]}:[ \t]*\r?$", m => m.Groups[1].Value + newLabel + ": // " + labels[i], RegexOptions.Multiline);
            //        code = Regex.Replace(code, $@"^([ \t]*JR.*[\t ]){labels[i]}[ \t]*\r?$", m => m.Groups[1].Value + newLabel, RegexOptions.Multiline);
            //    }
            //}
            //return code;
        }

        protected string NormalizeInlineTemps(string code)
        {
            var tmps = Regex.Matches(code, $@"\bDATA(?:F|8|32) ((tmp|tmpf|flag|milliseconds|timer|layer|nos|busy|no|mode)[0-9]+)\b", RegexOptions.Multiline).OfType<Match>()
                .Select(m => new { VarName = m.Groups[1].Value, VarPrefix = m.Groups[2].Value })
                .ToArray();
            for (int i = 0; i < tmps.Length; i++)
            {
                string newTmp = $"DUMMY___{tmps[i].VarPrefix}{i}";
                code = Regex.Replace(code, $@"\b{tmps[i].VarName}\b", newTmp, RegexOptions.Singleline);
            }

            return code.Replace("DUMMY___", "");
        }

        protected string NormalizeTemps(string code)
        {
            code = NormalizeTemps(code, "F");
            code = NormalizeTemps(code, "S");
            return code.Replace("DUMMY___", "");
        }

        private string NormalizeTemps(string code, string varType)
        {
            int startOfProg = code.IndexOf("MOVE8_8 0 STACKPOINTER");
            var tmpDeclarations = Regex.Matches(code, $@"\bDATA{varType} ({varType}[0-9]+)\b", RegexOptions.Multiline).OfType<Match>()
                .Select(m => m.Groups[1].Value)
                .OrderBy(v => code.IndexOf(v, startOfProg))
                .ToArray();
            int id = 0;
            foreach (var tmpDeclaration in tmpDeclarations)
            {
                string newTmp = $"DUMMY___{varType}{id}";
                code = Regex.Replace(code, $@"\b{tmpDeclaration}\b", newTmp, RegexOptions.Singleline);
                id++;
            }
            return code;
        }

        protected string NormalizeReferences(string code)
        {
            Match match = Regex.Match(code, @"(.*ENDTHREAD:[^}]*}[ \r\n\t]*)(.*)", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value +
                    string.Join(Environment.NewLine, Regex.Matches(match.Groups[2].ToString(), "subcall[^/]*").OfType<Match>().Select(m => m.Value.Trim()).OrderBy(s => s));
            }
            return code;
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

        protected string ExtractReferenceDefinitionsCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, @"ENDTHREAD:[^}]*}(.*)", RegexOptions.Singleline);
            return string.Join(Environment.NewLine, Regex.Matches(match.Groups[1].ToString(), "subcall[^/]*").OfType<Match>().Select(m => m.Value.Trim()).OrderBy(s => s));
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

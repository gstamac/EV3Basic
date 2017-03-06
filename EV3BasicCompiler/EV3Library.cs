using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler
{
    public class EV3Library
    {
        private readonly Dictionary<string, string> modules;
        private readonly List<string> globals;
        private string runtimeInit;
        private readonly List<EV3SubDefinitionBase> subroutines;
        private int currentLineNo = 0;

        public List<Error> Errors { get; private set; }

        public EV3Library()
        {
            modules = new Dictionary<string, string>();
            globals = new List<string>();
            runtimeInit = "";
            subroutines = new List<EV3SubDefinitionBase>();
            Errors = new List<Error>();
        }

        public void Clear()
        {
            modules.Clear();
            globals.Clear();
            runtimeInit = "";
            subroutines.Clear();
            Errors.Clear();
        }

        public void LoadModule(string moduleSource)
        {
            currentLineNo = 0;
            using (StringReader reader = new StringReader(moduleSource))
            {
                String line;
                while ((line = ReadLine(reader)) != null)
                {
                    string lineWithoutComment = RemoveComment(line);
                    string cleanLine = lineWithoutComment.Trim();
                    if (cleanLine.StartsWith("subcall"))
                    {
                        LoadSub(line, reader);
                    }
                    else if (cleanLine.StartsWith("inline"))
                    {
                        LoadInline(line, reader);
                    }
                    else if (cleanLine.StartsWith("init"))
                    {
                        LoadInit(reader);
                    }
                    else if (!string.IsNullOrEmpty(cleanLine))
                    {
                        globals.Add(lineWithoutComment);
                    }
                }
            }
        }

        public EV3SubDefinitionBase FindSubroutine(string subroutineName)
        {
            return subroutines.FirstOrDefault(s => s.Name.Equals(subroutineName, StringComparison.InvariantCultureIgnoreCase));
        }

        public Dictionary<string, EV3Type> GetSubResultTypes()
        {
            Dictionary<string, EV3Type> types = new Dictionary<string, EV3Type>();

            foreach (EV3SubDefinitionBase sub in subroutines)
            {
                if (!types.ContainsKey(sub.Name))
                    types.Add(sub.Name, sub.ReturnType);
            }

            return types;
        }

        public void CompileCodeForGlobals(TextWriter writer)
        {
            foreach (string line in globals)
            {
                writer.WriteLine(line);
            }
        }

        public void CompileCodeForRuntimeInit(TextWriter writer)
        {
            writer.WriteLine(runtimeInit);
        }

        public void CompileCodeForReferences(TextWriter writer)
        {
            List<string> references = new List<string>();
            string code = writer.ToString();
            while (true)
            {
                MatchCollection matches = Regex.Matches(code, "CALL[ \t]+([^ \t\n\r]+)[ \t\n\r/]", RegexOptions.Singleline);
                List<string> newReferences = matches.OfType<Match>().Select(m => m.Groups[1].Value).Distinct().Except(references).ToList();
                if (newReferences.Count == 0) break;

                references.AddRange(newReferences);
                code = "";
                foreach (EV3SubcallDefinition sub in newReferences.Select(s => FindSubroutine(s)).Where(s => s != null))
                {
                    code += Environment.NewLine + sub.Signature + Environment.NewLine + "{" + Environment.NewLine + sub.Code + Environment.NewLine + "}" + Environment.NewLine;
                }
                writer.Write(code);
            }
        }

        private void LoadSub(string line, StringReader reader)
        {
            AddSub(EV3SubcallDefinition.Create(line, GetBlock(reader)), line);
        }

        private void LoadInline(string line, StringReader reader)
        {
            AddSub(EV3InlineDefinition.Create(line, GetBlock(reader)), line);
        }

        private void AddSub(EV3SubDefinitionBase sub, string line)
        {
            if (sub != null)
            {
                if (sub.ParameterTypes.Any(p => p == EV3Type.Unknown) || sub.ReturnType == EV3Type.Unknown)
                    AddError($"Unknow parameter type for sub ({line})");
                else
                    subroutines.Add(sub);
            }
            else
                AddError($"Unknow sub definition ({line})");
        }

        private void LoadInit(StringReader reader)
        {
            runtimeInit += GetBlock(reader);
        }

        private string GetBlock(StringReader reader)
        {
            StringWriter block = new StringWriter();
            String line;
            while ((line = ReadLine(reader)) != null)
            {
                string lineWithoutComment = RemoveComment(line);
                string cleanLine = lineWithoutComment.Trim();
                if (cleanLine.Equals("}"))
                    return block.ToString();
                if (!cleanLine.Equals("{"))
                    block.WriteLine(lineWithoutComment);
            }

            return "";
        }

        private string ReadLine(StringReader reader)
        {
            currentLineNo++;
            return reader.ReadLine()?.Replace("\t", "    ");
        }

        private static string RemoveComment(string line)
        {
            return Regex.Replace(line, "[ \t]*//.*", "");
        }

        private void AddError(string message)
        {
            Errors.Add(new Error(message, currentLineNo, 0));
        }

    }
}

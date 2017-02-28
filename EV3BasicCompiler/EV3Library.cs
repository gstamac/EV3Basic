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
        private readonly List<EV3SubDefinition> subroutines;
        private readonly List<EV3SubDefinition> inlines;
        private int currentLineNo = 0;

        public List<Error> Errors { get; private set; }

        public EV3Library()
        {
            modules = new Dictionary<string, string>();
            globals = new List<string>();
            runtimeInit = "";
            subroutines = new List<EV3SubDefinition>();
            inlines = new List<EV3SubDefinition>();
            Errors = new List<Error>();
        }

        public void Clear()
        {
            modules.Clear();
            globals.Clear();
            runtimeInit = "";
            subroutines.Clear();
            inlines.Clear();
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
                    string cleanLine = CleanupLine(line);
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
                        LoadInit(line, reader);
                    }
                    else if (!string.IsNullOrEmpty(cleanLine.Trim()))
                    {
                        globals.Add(line);
                    }
                }
            }
        }

        public EV3SubDefinition FindSubroutine(string subroutineName)
        {
            return subroutines.FirstOrDefault(s => s.Name.Equals(subroutineName, StringComparison.InvariantCultureIgnoreCase));
        }

        public EV3SubDefinition FindInline(string subroutineName)
        {
            return inlines.FirstOrDefault(s => s.Name.Equals(subroutineName, StringComparison.InvariantCultureIgnoreCase));
        }

        public Dictionary<string, EV3Type> GetSubResultTypes()
        {
            Dictionary<string, EV3Type> types = new Dictionary<string, EV3Type>();

            foreach (EV3SubDefinition sub in subroutines)
            {
                if (!types.ContainsKey(sub.Name))
                    types.Add(sub.Name, sub.ReturnType);
            }
            foreach (EV3SubDefinition sub in inlines)
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
            foreach (EV3SubDefinition sub in subroutines.Where(s => s.Referenced))
            {
                writer.WriteLine();
                writer.WriteLine(sub.Code);
            }
        }

        private void LoadSub(string line, StringReader reader)
        {
            EV3SubDefinition sub = ParseSubDefinition(line, reader);

            if (sub != null)
                subroutines.Add(sub);
        }

        private void LoadInline(string line, StringReader reader)
        {
            EV3SubDefinition sub = ParseSubDefinition(line, reader);

            if (sub != null)
                inlines.Add(sub);
        }

        private EV3SubDefinition ParseSubDefinition(string line, StringReader reader)
        {
            EV3SubDefinition sub = null;
            Match match = Regex.Match(line, "(subcall|inline)[ \t]*([^ \t]+)[ \t]*//[ \t]*([SFAX8]*)([SFAX8V])([ \t]+([^ \t\n\r]+))*", RegexOptions.Singleline);
            if (match.Success)
            {
                sub = new EV3SubDefinition(match.Groups[2].Value, line, GetBlock(line, reader));

                if (match.Groups[1].Value == "inline")
                {
                    sub.ParseParameterTypes(match.Groups[3].Value);
                    sub.ParseReturnType(match.Groups[4].Value);

                    //sub.ParseParameters();
                    //List<EV3Type> parameterTypes = ParseParameterTypes(match.Groups[3].Value);
                    //EV3Type returnType = EV3SubDefinition.ParseType(match.Groups[4].Value);
                    //if (sub.ReturnType != returnType)
                    //    AddError($"Return types do not match for sub ({line}) " + returnType);
                    //if (parameterTypes.Count != sub.ParameterTypes.Count || !parameterTypes.Select((pt, i) => pt == sub.ParameterTypes[i]).All(t => t))
                    //    AddError($"Parameter types do not match for sub ({line}) {string.Join(", ", parameterTypes)}");
                }

                //List<string> references = new List<string>();
                //if (match.Groups[6].Captures.Count > 0)
                //    references = match.Groups[6].Captures.OfType<Capture>().Select(c => c.Value).ToList();
                //if (sub.References.Count != references.Count || (references.Count > 0 && !references.All(r =>
                //{
                //    if (sub.References.Contains(r))
                //    return true;
                //    AddError($"    REF {r} MISSING");
                //    return false;
                //})))
                //{

                //    AddError($"References missing for sub ({line}) {sub.References.Count}'{string.Join(",", sub.References)}' != {references.Count}'{string.Join(",", references)}'");
                //    for (int j = 0; j < match.Groups[6].Captures.Count; j++)
                //    {
                //        AddError($"    CAPTURE {j} {match.Groups[6].Captures[j].Value}");
                //    }
                //}

                if (sub.ParameterTypes.Any(p => p == EV3Type.Unknown) || sub.ReturnType == EV3Type.Unknown)
                    AddError($"Unknow parameter type for sub ({line})");
                else
                    return sub;
            }
            else
                AddError($"Unknow sub definition ({line})");

            return null;
        }

        private void LoadInit(string line, StringReader reader)
        {
            runtimeInit += GetBlock(line, reader);
        }

        private string GetBlock(string signature, StringReader reader)
        {
            StringWriter block = new StringWriter();
            String line;
            while ((line = ReadLine(reader)) != null)
            {
                if (CleanupLine(line).Equals("}"))
                    return block.ToString();
                if (!CleanupLine(line).Equals("{"))
                    block.WriteLine(line);
            }

            return "";
        }

        private string ReadLine(StringReader reader)
        {
            currentLineNo++;
            return reader.ReadLine()?.Replace("\t", "    ");
        }

        private static string CleanupLine(string line)
        {
            line = Regex.Replace(line, "//.*", "").Trim();
            return line.Trim();
        }

        private void AddError(string message)
        {
            Errors.Add(new Error(message, currentLineNo, 0));
        }

    }
}

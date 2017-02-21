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
        private readonly List<string> runtimeInit;
        private readonly List<EV3SubDefinition> subs;
        private readonly List<EV3SubDefinition> inlines;
        private int currentLineNo = 0;

        public EV3Library()
        {
            modules = new Dictionary<string, string>();
            globals = new List<string>();
            runtimeInit = new List<string>();
            subs = new List<EV3SubDefinition>();
            inlines = new List<EV3SubDefinition>();
            Errors = new List<Error>();
        }

        public List<Error> Errors { get; private set; }

        public void Clear()
        {
            modules.Clear();
            globals.Clear();
            runtimeInit.Clear();
            subs.Clear();
            inlines.Clear();
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

        public Dictionary<string, EV3Type> GetSubResultTypes()
        {
            Dictionary<string, EV3Type> types = new Dictionary<string, EV3Type>();

            foreach (EV3SubDefinition sub in subs)
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

        public void GenerateCodeForGlobals(TextWriter writer)
        {
            foreach (string line in globals)
            {
                writer.WriteLine(line);
            }
        }

        public void GenerateCodeForRuntimeInit(TextWriter writer)
        {
            foreach (string line in runtimeInit)
            {
                writer.WriteLine(line);
            }
        }

        private void LoadSub(string line, StringReader reader)
        {
            EV3SubDefinition sub = ParseSubDefinition(line, reader);

            if (sub != null)
                subs.Add(sub);
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
            Match match = Regex.Match(line, "(subcall|inline)[ \t]*([^ \t]+)[ \t]*//[ \t]*([SFAX8]*)([SFAX8V])");
            if (match.Success)
            {
                sub = new EV3SubDefinition
                {
                    Name = match.Groups[2].Value,
                    Code = GetBlock(reader)
                };
                if (match.Groups[1].Value == "subcall")
                {
                    sub.ParseParameters();
                }
                else
                {
                    sub.ParameterTypes = ParseParameterTypes(match.Groups[3].Value);
                    sub.ReturnType = EV3SubDefinition.ParseType(match.Groups[4].Value);
                }
                if (sub.ParameterTypes.Any(p => p == EV3Type.Unknown) || sub.ReturnType == EV3Type.Unknown)
                    AddError($"Unknow parameter type for sub ({line})");
                //else if (sub.ReturnType != sub.ReturnType2)
                //    AddError($"Return types do not match for sub ({line}) " + sub.ReturnType2);
                //else if (sub.ParameterTypes2.Count != sub.ParameterTypes.Count || !sub.ParameterTypes2.Select((pt, i) => pt == sub.ParameterTypes[i]).All(t => t))
                //    AddError($"Parameter types do not match for sub ({line}) {string.Join(", ", sub.ParameterTypes2)}");
                return sub;
            }
            else
                AddError($"Unknow sub definition ({line})");

            return null;
        }

        private List<EV3Type> ParseParameterTypes(string value)
        {
            return value.Select(c => EV3SubDefinition.ParseType(c.ToString())).ToList();
        }

        private void LoadInit(string line, StringReader reader)
        {
            List<string> block = GetBlock(reader);
            runtimeInit.AddRange(block.Skip(1).Take(block.Count - 2));
        }

        private List<string> GetBlock(StringReader reader)
        {
            List<string> block = new List<string>();
            String line;
            while ((line = ReadLine(reader)) != null)
            {
                block.Add(line);
                if (CleanupLine(line).Equals("}"))
                    return block;
            }

            return block;
        }

        private string ReadLine(StringReader reader)
        {
            currentLineNo++;
            return reader.ReadLine();
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

        class EV3SubDefinition
        {
            public string Name { get; set; }
            public List<EV3Type> ParameterTypes { get; set; }
            public EV3Type ReturnType { get; set; }
            public List<string> Code { get; set; }

            public void ParseParameters()
            {
                ParameterTypes = new List<EV3Type>();
                ReturnType = EV3Type.Void;

                Dictionary<string, EV3Type> parameters = new Dictionary<string, EV3Type>();

                string codeString = string.Join("\n", Code);

                MatchCollection matches = Regex.Matches(codeString, "IN_([^ \t]+)[ \t]+([^ \t\n\r]+)[ \t\n\r/]", RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    parameters.Add(match.Groups[2].Value, ParseType(match.Groups[1].Value));
                }

                matches = Regex.Matches(codeString, "OUT_([^ \t]+)[ \t]+", RegexOptions.Singleline);
                if (matches.Count == 1)
                {
                    ReturnType = ParseType(matches[0].Groups[1].Value);
                }
                else
                {
                    if (parameters.Count > 0 && parameters.Values.Last() == EV3Type.Int16 
                        && Regex.IsMatch(codeString, "(ARRAY_WRITE|ARRAY FILL|(ARRAY WRITE_CONTENT[ \t]+[^ \t]+)|(ARRAY COPY[ \t]+[^ \t]+))[ \t]+" + parameters.Keys.Last() + "[ \t\n\r/]+", RegexOptions.Singleline))
                    {
                        ReturnType = EV3Type.FloatArray;
                        parameters.Remove(parameters.Keys.Last());
                    }
                }
                foreach (string name in parameters.Where(kv => kv.Value == EV3Type.Int16).Select(kv => kv.Key).ToArray())
                {
                    if (Regex.IsMatch(codeString, "(ARRAY_READ|ARRAY COPY)[ \t]+" + name + "[ \t]+", RegexOptions.Singleline))
                    {
                        parameters[name] = EV3Type.FloatArray;
                    }
                }

                ParameterTypes = parameters.Values.Select(pt => NormalizeType(pt)).ToList();
                ReturnType = NormalizeType(ReturnType);
            }

            public static EV3Type ParseType(string typeString)
            {
                switch (typeString)
                {
                    case "8":
                        return EV3Type.Int8;
                    case "16":
                        return EV3Type.Int16;
                    case "32":
                        return EV3Type.Int32;
                    case "F":
                        return EV3Type.Float;
                    case "S":
                        return EV3Type.String;
                    case "A":
                        return EV3Type.FloatArray;
                    case "X":
                        return EV3Type.StringArray;
                    case "V":
                        return EV3Type.Void;
                }
                return EV3Type.Unknown;
            }

            private EV3Type NormalizeType(EV3Type pt)
            {
                switch (pt)
                {
                    case EV3Type.Int8:
                    case EV3Type.Int16:
                    case EV3Type.Int32:
                        return EV3Type.Float;
                    case EV3Type.Int8Array:
                    case EV3Type.Int16Array:
                    case EV3Type.Int32Array:
                        return EV3Type.FloatArray;
                }
                return pt;
            }
        }
    }
}

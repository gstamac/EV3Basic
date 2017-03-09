using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler
{
    public class EV3MethodDefinition : EV3SubDefinitionBase
    {
        private EV3MethodDefinition(string name, string signature, string code) : base(name, signature, code)
        {
            ParseParameters();
        }

        public static EV3MethodDefinition Create(string signature, string code)
        {
            Match match = Regex.Match(signature, "subcall[ \t]*([^ \t]+)[ \t]*", RegexOptions.Singleline);
            if (match.Success)
            {
                return new EV3MethodDefinition(match.Groups[1].Value, signature, code);
            }
            return null;
        }

        public override string Compile(TextWriter writer, EV3CompilerContext context, string[] arguments, string result)
        {
            string argumentsString = string.Join("", arguments.Select(a => " " + a));
            if (result == null)
                writer.WriteLine($"    CALL {Name}{argumentsString}");
            else
                writer.WriteLine($"    CALL {Name}{argumentsString} {result}");

            return result;
        }

        private void ParseParameters()
        {
            ParameterTypes = new List<EV3Type>();
            ReturnType = EV3Type.Void;

            Dictionary<string, EV3Type> parameters = new Dictionary<string, EV3Type>();

            MatchCollection matches = Regex.Matches(Code, "IN_([^ \t]+)[ \t]+([^ \t\n\r]+)[ \t\n\r/]", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                parameters.Add(match.Groups[2].Value, ParseType(match.Groups[1].Value));
            }

            matches = Regex.Matches(Code, "OUT_([^ \t]+)[ \t]+", RegexOptions.Singleline);
            if (matches.Count == 1)
            {
                ReturnType = ParseType(matches[0].Groups[1].Value);
            }
            else
            {
                if (parameters.Count > 0 && parameters.Values.Last() == EV3Type.Int16
                    && Regex.IsMatch(Code, "(ARRAY_WRITE|ARRAY FILL|(ARRAY WRITE_CONTENT[ \t]+[^ \t]+)|(ARRAY COPY[ \t]+[^ \t]+))[ \t]+" + parameters.Keys.Last() + "[ \t\n\r/]+", RegexOptions.Singleline))
                {
                    ReturnType = EV3Type.FloatArray;
                    parameters.Remove(parameters.Keys.Last());
                }
            }
            foreach (string name in parameters.Where(kv => kv.Value == EV3Type.Int16).Select(kv => kv.Key).ToArray())
            {
                if (Regex.IsMatch(Code, "(ARRAY_READ|ARRAY COPY)[ \t]+" + name + "[ \t]+", RegexOptions.Singleline))
                {
                    parameters[name] = EV3Type.FloatArray;
                }
            }

            ParameterTypes = parameters.Values.Select(pt => NormalizeType(pt)).ToList();
            ReturnType = NormalizeType(ReturnType);
        }

        protected EV3Type NormalizeType(EV3Type pt)
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

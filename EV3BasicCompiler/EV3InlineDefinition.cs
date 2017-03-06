using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler
{
    public class EV3InlineDefinition : EV3SubDefinitionBase
    {
        private EV3InlineDefinition(string name, string signature, string code) : base(name, signature, code)
        {
        }

        public static EV3InlineDefinition Create(string signature, string code)
        {
            Match match = Regex.Match(signature, "inline[ \t]*([^ \t]+)[ \t]*//[ \t]*([SFAX8]*)([SFAX8V])([ \t]+([^ \t\n\r]+))*", RegexOptions.Singleline);
            if (match.Success)
            {
                EV3InlineDefinition sub = new EV3InlineDefinition(match.Groups[1].Value, signature, code);
                sub.ParseParameterTypes(match.Groups[2].Value);
                sub.ParseReturnType(match.Groups[3].Value);
                return sub;
            }
            return null;
        }

        private void ParseParameterTypes(string value)
        {
            ParameterTypes = value.Select(c => ParseType(c.ToString())).ToList();
        }

        private void ParseReturnType(string value)
        {
            ReturnType = ParseType(value);
        }

        public override string Compile(TextWriter writer, EV3CompilerContext context, string[] arguments, string result)
        {
            string inlineCode = Code;

            for (int i = 0; i < arguments.Length; i++)
                inlineCode = inlineCode.Replace($":{i}", arguments[i]);
            if (result != null)
                inlineCode = inlineCode.Replace($":{arguments.Length}", result);
            if (inlineCode.Contains(":#"))
                inlineCode = inlineCode.Replace(":#", context.GetNextLabelNumber().ToString());

            if (Code.Equals(inlineCode))
            {
                inlineCode = inlineCode.TrimEnd(' ', '\t', '\n', '\r') + " " + string.Join(" ", arguments);
                if (result != null)
                    inlineCode += " " + result;
            }

            writer.WriteLine(inlineCode);

            return result;
        }
    }
}

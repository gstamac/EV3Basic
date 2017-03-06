using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using EV3BasicCompiler.Compilers;
using System;

namespace EV3BasicCompiler
{
    public class EV3SubDefinition : EV3SubDefinitionBase
    {
        public EV3SubDefinition(string name, string signature, string code) : base(name, signature, code)
        {
        }

        public override string Compile(TextWriter writer, EV3CompilerContext context, string[] arguments, string result)
        {
            writer.WriteLine($"SUB_{Name}:");
            writer.WriteLine(Code);
            writer.WriteLine($"ENDSUB_{Name}:");

            return "";
        }
    }
}

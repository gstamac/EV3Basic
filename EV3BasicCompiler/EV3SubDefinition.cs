using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using EV3BasicCompiler.Compilers;
using System;

namespace EV3BasicCompiler
{
    public class EV3SubDefinition : EV3SubDefinitionBase
    {
        private readonly Func<string> codeGenerator;

        private string code;
        public override string Code
        {
            get
            {
                EnsureCode();
                return code;
            }
        }

        public EV3SubDefinition(string name, string signature, Func<string> codeGenerator) : base (name, signature, "")
        {
            this.codeGenerator = codeGenerator;
        }

        public override string Compile(TextWriter writer, EV3CompilerContext context, string[] arguments, string result)
        {
            writer.WriteLine($"SUB_{Name}:");
            writer.Write(Code);
            writer.WriteLine($"ENDSUB_{Name}:");

            return "";
        }

        private bool isReferenced;
        public bool IsReferenced
        {
            get { return isReferenced; }
            set
            {
                if (value) EnsureCode();
                isReferenced = value;
            }
        }

        private void EnsureCode()
        {
            if (code == null)
            {
                code = "";
                code = codeGenerator();
            }
        }
    }
}

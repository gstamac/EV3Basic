using System;
using Microsoft.SmallBasic;
using System.Collections.Generic;

namespace EV3BasicCompiler.Compilers
{
    public class EV3CompilerContext
    {
        private readonly EV3Variables variables;
        private EV3Library library;

        public List<Error> Errors { get; private set; }

        public EV3CompilerContext(EV3Variables variables, EV3Library library)
        {
            this.variables = variables;
            this.library = library;

            Errors = new List<Error>();
        }

        public EV3Variable FindVariable(string name)
        {
            return variables.FindVariable(name);
        }

        public EV3SubDefinition FindSubroutine(string subroutineName)
        {
            return library.FindSubroutine(subroutineName);
        }

        public void AddError(string message, int line, int column)
        {
            Errors.Add(new Error(message, line, column));
        }

        public void AddError(string message, TokenInfo tokenInfo)
        {
            Errors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
        }
    }
}

using System;
using Microsoft.SmallBasic;
using System.Collections.Generic;

namespace EV3BasicCompiler.Compilers
{
    public class EV3CompilerContext
    {
        private const int MAX_TEMP_VARIABLE_INDEX = 255;

        private readonly EV3Variables variables;
        private EV3Library library;

        public bool DoDivisionCheck { get; set; }
        public bool DoBoundsCheck { get; set; }

        public List<Error> Errors { get; private set; }
        public List<Error> CompileErrors { get; private set; }

        public EV3CompilerContext(EV3Variables variables, EV3Library library)
        {
            this.variables = variables;
            this.library = library;

            DoDivisionCheck = true;
            DoBoundsCheck = true;

            Errors = new List<Error>();
            CompileErrors = new List<Error>();
        }

        public EV3Variable FindVariable(string name)
        {
            return variables.FindVariable(name);
        }

        public EV3SubDefinition FindSubroutine(string subroutineName)
        {
            return library.FindSubroutine(subroutineName);
        }

        public IEV3Variable CreateTempVariable(EV3Type type, TokenInfo tokenInfo)
        {
            return variables.CreateTempVariable(type, tokenInfo);
        }

        public void RemoveTempVariable(IEV3Variable variable)
        {
            variables.RemoveTempVariable(variable);
        }

        public void AddError(string message, int line, int column)
        {
            Errors.Add(new Error(message, line, column));
        }

        public void AddError(string message, TokenInfo tokenInfo)
        {
            AddError(message, tokenInfo.Line + 1, tokenInfo.Column + 1);
        }

        public void AddCompileError(string message, int line, int column)
        {
            CompileErrors.Add(new Error(message, line, column));
        }

        public void AddCompileError(string message, TokenInfo tokenInfo)
        {
            AddCompileError(message, tokenInfo.Line + 1, tokenInfo.Column + 1);
        }
    }
}

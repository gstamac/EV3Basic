using Microsoft.SmallBasic;
using System.Collections.Generic;
using Microsoft.SmallBasic.Statements;

namespace EV3BasicCompiler.Compilers
{
    public class EV3CompilerContext
    {
        private const int MAX_TEMP_VARIABLE_INDEX = 255;

        private int nextLabel;
        private int nextThread;
        private readonly EV3Variables variables;
        private EV3MainProgram mainProgram;
        private EV3Library library;

        public bool DoDivisionCheck { get; set; }
        public bool DoBoundsCheck { get; set; }
        public bool DoOptimization { get; set; }

        public List<Error> Errors { get; private set; }

        public EV3CompilerContext(EV3Variables variables, EV3MainProgram mainProgram, EV3Library library)
        {
            this.variables = variables;
            this.mainProgram = mainProgram;
            this.library = library;

            DoDivisionCheck = true;
            DoBoundsCheck = true;
            DoOptimization = false;

            nextLabel = 0;
            nextThread = 0;

            Errors = new List<Error>();
        }

        public int GetNextLabelNumber()
        {
            return nextLabel++;
        }

        public int GetNextThreadNumber()
        {
            return nextThread++;
        }

        public EV3Variable FindVariable(string name)
        {
            return variables.FindVariable(name);
        }

        public EV3Variable FindVariable(ForStatement forStatement)
        {
            return variables.FindVariable(forStatement);
        }

        public EV3SubDefinitionBase FindMethod(string methodName)
        {
            return library.FindMethod(methodName);
        }

        public EV3SubDefinition FindSubroutine(string subroutineName)
        {
            return mainProgram.FindSubroutine(subroutineName);
        }

        public EV3Variables.TempVariableCreator UseTempVariables()
        {
            return variables.UseTempVariables();
        }

        public void AddError(string message, int line, int column)
        {
            Errors.Add(new Error(message, line, column));
        }

        public void AddError(string message, TokenInfo tokenInfo)
        {
            AddError(message, tokenInfo.Line + 1, tokenInfo.Column + 1);
        }
    }
}

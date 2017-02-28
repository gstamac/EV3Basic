using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class IdentifierExpressionCompiler : ExpressionCompiler<IdentifierExpression>, IAssignmentExpressionCompiler
    {
        public IdentifierExpressionCompiler(IdentifierExpression expression, EV3CompilerContext context) : base(expression, context)
        {
            index = -2;
        }

        protected override void CalculateType()
        {
            EV3Variable reference = Context.FindVariable(VariableName);
            type = reference.Type;
        }

        protected override void CalculateValue()
        {
            IEV3Variable reference = Context.FindVariable(VariableName);

            if (reference != null)
                value = reference.Ev3Name;
            else
                value = VariableName;
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            return Value;
        }

        public void CompileAssignment(TextWriter writer, string compiledValue, string outputName)
        {
            if (compiledValue.Equals(outputName, StringComparison.InvariantCultureIgnoreCase)) return;

            if (Type.IsArray())
            {
                writer.WriteLine($"    ARRAY COPY {compiledValue} {outputName}");
            }
            else
            {
                switch (Type.BaseType())
                {
                    case EV3Type.Int8:
                        writer.WriteLine($"    MOVE8_8 {compiledValue} {outputName}");
                        break;
                    case EV3Type.Int16:
                        writer.WriteLine($"    MOVE16_16 {compiledValue} {outputName}");
                        break;
                    case EV3Type.Int32:
                        writer.WriteLine($"    MOVE32_32 {compiledValue} {outputName}");
                        break;
                    case EV3Type.Float:
                        writer.WriteLine($"    MOVEF_F {compiledValue} {outputName}");
                        break;
                    case EV3Type.String:
                        writer.WriteLine($"    STRINGS DUPLICATE {compiledValue} {outputName}");
                        break;
                }
            }
        }

        private string _variableName = null;
        public string VariableName
        {
            get
            {
                if (_variableName == null)
                    _variableName = ParentExpression.VariableName();
                return _variableName;
            }
        }

        private int index;
        public int Index
        {
            get
            {
                if (index == -2)
                {
                    EV3Variable reference = Context.FindVariable(VariableName);
                    index = reference.MaxIndex;
                }
                return index;
            }
        }
    }
}

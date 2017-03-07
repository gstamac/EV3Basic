using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class IdentifierExpressionCompiler : ExpressionCompiler<IdentifierExpression>, IAssignmentExpressionCompiler, IBooleanExpressionCompiler
    {
        public IdentifierExpressionCompiler(IdentifierExpression expression, EV3CompilerContext context) : base(expression, context)
        {
            index = -2;
        }

        protected override EV3Type CalculateType()
        {
            return Variable.Type;
        }

        protected override bool CalculateIsLiteral()
        {
            return Variable.IsConstant; 
        }

        protected override string CalculateValue()
        {
            if (Variable != null)
            {
                if (IsLiteral)
                    return Variable.Value;
                else
                    return Variable.Ev3Name;
            }
            else
                return VariableName;
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

        public void CompileBranch(TextWriter writer, string label)
        {
            CompileBranchForStringVariable(writer, this, label, false);
        }

        public void CompileBranchNegated(TextWriter writer, string label)
        {
            CompileBranchForStringVariable(writer, this, label, true);
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

        private EV3Variable variable;
        public EV3Variable Variable
        {
            get
            {
                if (variable == null)
                    variable = Context.FindVariable(VariableName);
                return variable;
            }
        }

        private int index;
        public int Index
        {
            get
            {
                if (index == -2)
                {
                    index = Variable.MaxIndex;
                }
                return index;
            }
        }

        public bool IsArray => Type.IsArray();
    }
}

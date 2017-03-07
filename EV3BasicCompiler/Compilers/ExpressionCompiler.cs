using Microsoft.SmallBasic;
using System;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System.IO;
using EV3BasicCompiler;

namespace EV3BasicCompiler.Compilers
{
    public abstract class ExpressionCompiler<T> : IExpressionCompiler where T : SBExpression
    {
        private EV3Type type;
        private bool? isLiteral;
        private string value;
        private bool? canCompileBoolean;
        private bool? booleanValue;

        public EV3CompilerContext Context { get; }
        protected T ParentExpression { get; }

        public ExpressionCompiler(T expression, EV3CompilerContext context)
        {
            ParentExpression = expression;
            Context = context;
            type = EV3Type.Unknown;
            value = null;
        }

        public virtual EV3Type Type
        {
            get
            {
                EnsureType();
                return type;
            }
        }

        public virtual bool IsLiteral
        {
            get
            {
                if (!isLiteral.HasValue) isLiteral = CalculateIsLiteral();
                return isLiteral.GetValueOrDefault();
            }
        }

        public virtual string Value
        {
            get
            {
                EnsureValue();
                return value;
            }
        }

        public virtual bool CanCompileBoolean
        {
            get
            {
                if (!canCompileBoolean.HasValue) canCompileBoolean = CalculateCanCompileBoolean();
                return canCompileBoolean.GetValueOrDefault();
            }
        }

        public virtual bool BooleanValue
        {
            get
            {
                if (!booleanValue.HasValue) booleanValue = CalculateBooleanValue();
                return booleanValue.GetValueOrDefault();
            }
        }

        protected void EnsureType()
        {
            if (type == EV3Type.Unknown) type = CalculateType();
        }

        protected void EnsureValue()
        {
            if (value == null) value = CalculateValue();
        }

        protected abstract EV3Type CalculateType();
        protected virtual bool CalculateIsLiteral() => false;
        protected virtual string CalculateValue() => null;
        protected virtual bool CalculateCanCompileBoolean() => (Type == EV3Type.Boolean) || (Type == EV3Type.String);
        protected virtual bool CalculateBooleanValue() => false;

        public abstract string Compile(TextWriter writer, IEV3Variable variable);

        protected string CompileWithConvert(TextWriter writer, IExpressionCompiler compiler, EV3Type resultType, EV3Variables.TempVariableCreator tempVariables)
        {
            string value = compiler.Compile(writer, tempVariables.CreateVariable(compiler.Type));

            if (compiler.Type.BaseType().IsNumber() && resultType.BaseType() == EV3Type.String)
            {
                if (compiler.IsLiteral)
                {
                    return "'" + SmallBasicExtensions.FormatFloat(value) + "'";
                }
                else
                {
                    IEV3Variable outputVariable = tempVariables.CreateVariable(EV3Type.String);

                    writer.WriteLine($"    STRINGS VALUE_FORMATTED {value} '%g' 99 {outputVariable.Ev3Name}");

                    return outputVariable.Ev3Name;
                }
            }
            else
                return value;
        }

        public void CompileBranchForStringVariable(TextWriter writer, IExpressionCompiler valueCompiler, string label, bool negated)
        {
            if (CanCompileBoolean)
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    IEV3Variable tempResultVariable = tempVariables.CreateVariable(EV3Type.String);
                    IEV3Variable tempIsTrueVariable = tempVariables.CreateVariable(EV3Type.Float);
                    writer.WriteLine($"    CALL IS_TRUE {valueCompiler.Compile(writer, tempResultVariable)} {tempIsTrueVariable.Ev3Name}");
                    writer.WriteLine($"    JR_{(negated ? "EQ" : "NEQ")}8 {tempIsTrueVariable.Ev3Name} 0 {label}");

                    //writer.WriteLine($"    STRINGS DUPLICATE {Value} {tempResultVariable.Ev3Name}");
                    //writer.WriteLine($"    AND8888_32 {tempResultVariable.Ev3Name} -538976289 {tempResultVariable.Ev3Name}");
                    //writer.WriteLine($"    STRINGS COMPARE {tempResultVariable.Ev3Name} 'TRUE' {tempIsTrueVariable.Ev3Name}");
                    //writer.WriteLine($"    JR_EQ8 {tempIsTrueVariable.Ev3Name} 0 {label}");
                }
            }
        }

        protected void AddError(string message) => AddError(message, ParentExpression.StartToken);
        protected void AddError(string message, TokenInfo tokenInfo) => Context.AddError(message, tokenInfo);

        public override string ToString() => $"{base.ToString()}[Type = {type}, IsLiteral = {isLiteral}, Value = {value}]";
    }
}

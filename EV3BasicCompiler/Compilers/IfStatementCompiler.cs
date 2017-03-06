using System.IO;
using Microsoft.SmallBasic.Statements;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EV3BasicCompiler.Compilers
{
    public class IfStatementCompiler : StatementCompiler<IfStatement>, IConditionStatementCompiler
    {
        public IfStatementCompiler(IfStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
            List<IConditionStatementCompiler> switchList = GenerateSwitchList();
            if (switchList != null)
            {
                CompileSwitchList(writer, switchList);
            }
            else
                AddError("Condition in if statement needs to be boolean expression");
        }

        private void CompileSwitchList(TextWriter writer, List<IConditionStatementCompiler> switchList)
        {
            if (switchList.Count == 0) return;

            if (switchList[0].IsAlwaysTrue)
            {
                switchList[0].CompileStatements(writer);
                return;
            }

            int label = Context.GetNextLabelNumber();
            int subLabel = 1;
            bool isFirst = true;
            foreach (IConditionStatementCompiler switchCompiler in switchList)
            {
                if (switchCompiler.IsAlwaysTrue)
                {
                    writer.WriteLine($"    JR endif{label}");
                    writer.WriteLine($"  else{label}_{subLabel}:");
                    subLabel++;
                }
                else
                {
                    if (!isFirst)
                    {
                        writer.WriteLine($"    JR endif{label}");
                        writer.WriteLine($"  else{label}_{subLabel}:");
                        subLabel++;
                    }
                    switchCompiler.ConditionCompiler.CompileBranchNegated(writer, $"else{label}_{subLabel}");
                }

                switchCompiler.CompileStatements(writer);

                if (switchCompiler.IsAlwaysTrue) break;

                isFirst = false;
            }
            writer.WriteLine($"  else{label}_{subLabel}:");
            writer.WriteLine($"  endif{label}:");
        }

        public void CompileStatements(TextWriter writer)
        {
            ParentStatement.ThenStatements.Compile(writer);
        }

        private List<IConditionStatementCompiler> GenerateSwitchList()
        {
            List<IConditionStatementCompiler> list = new List<IConditionStatementCompiler>();

            list.Add(this);
            list.AddRange(ParentStatement.ElseIfStatements.Select(s => s.Compiler<ElseIfStatementCompiler>()));
            if (list.Any(c => (c.ConditionCompiler == null) || !c.ConditionCompiler.CanCompileBoolean))
                return null;

            if (ParentStatement.ElseStatements.Count > 0)
                list.Add(new ElseStatementCompiler(ParentStatement.ElseStatements));

            list.RemoveAll(c => c.IsAlwaysFalse);

            IConditionStatementCompiler last = list.FirstOrDefault(c => c.IsAlwaysTrue);
            if (last != null)
            {
                int index = list.IndexOf(last);
                list.RemoveRange(index + 1, list.Count - index - 1);
            }

            return list;
        }

        class ElseStatementCompiler : IConditionStatementCompiler
        {
            private readonly List<Statement> elseStatements;

            public ElseStatementCompiler(List<Statement> elseStatements)
            {
                this.elseStatements = elseStatements;
            }

            public IBooleanExpressionCompiler ConditionCompiler { get { return null; } }

            public bool IsAlwaysFalse { get { return false; } }
            public bool IsAlwaysTrue { get { return true; } }

            public void CompileStatements(TextWriter writer)
            {
                elseStatements.Compile(writer);
            }
        }

        public bool IsAlwaysFalse { get { return IsLiteralCondition() && !ConditionCompiler.BooleanValue; } }
        public bool IsAlwaysTrue { get { return IsLiteralCondition() && ConditionCompiler.BooleanValue; } }

        private bool IsLiteralCondition()
        {
            return (ConditionCompiler != null) && ConditionCompiler.CanCompileBoolean && ConditionCompiler.IsLiteral;
        }

        private IBooleanExpressionCompiler conditionCompiler;
        public IBooleanExpressionCompiler ConditionCompiler
        {
            get
            {
                if (conditionCompiler == null)
                    conditionCompiler = ParentStatement.Condition.Compiler<IBooleanExpressionCompiler>();
                return conditionCompiler;
            }
        }
    }
}

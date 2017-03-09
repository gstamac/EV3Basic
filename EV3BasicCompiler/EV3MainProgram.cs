using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.SmallBasic.Expressions;
using EV3BasicCompiler.Compilers;
using EV3BasicCompiler;

namespace EV3BasicCompiler
{
    public class EV3MainProgram
    {
        private readonly Parser parser;
        private List<ThreadStatementCompiler> threadCompilers;
        private readonly List<EV3SubDefinition> subroutines;

        public List<Error> Errors { get; private set; }

        public EV3MainProgram(Parser parser)
        {
            this.parser = parser;

            subroutines = new List<EV3SubDefinition>();

            Errors = new List<Error>();

            Clear();
        }

        public void Clear()
        {
            subroutines.Clear();
            Errors.Clear();
        }

        public void Process(EV3CompilerContext context)
        {
            ProcessThreads();
            ProcessSubroutines(context);
        }

        private void ProcessThreads()
        {
            threadCompilers = parser.GetStatements<AssignmentStatement>().Select(s => s.Compiler()).OfType<ThreadStatementCompiler>().ToList();
        }

        private void ProcessSubroutines(EV3CompilerContext context)
        {
            foreach (SubroutineStatementCompiler compiler in parser.GetStatements<SubroutineStatement>().Select(s => s.Compiler<SubroutineStatementCompiler>()))
            {
                EV3SubDefinition sub = new EV3SubDefinition(compiler.Ev3Name, compiler.Ev3Name, () =>
                {
                    StringWriter code = new StringWriter();
                    compiler.Compile(code, true);
                    return code.ToString();
                });
                if (threadCompilers.Any(t => t.ThreadName.Equals(compiler.Ev3Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    foreach (SBExpression expression in compiler.ParentStatement.SubroutineBody.GetExpressions())
                    {
                        if (expression is IdentifierExpression)
                            context.FindVariable(((IdentifierExpression)expression).VariableName()).IsConstant = false;
                        else if (expression is ArrayExpression)
                            context.FindVariable(((ArrayExpression)expression).VariableName()).IsConstant = false;
                    }
                }
                subroutines.Add(sub);
            }
        }

        public EV3SubDefinition FindSubroutine(string subroutineName)
        {
            return subroutines.FirstOrDefault(s => s.Name.Equals(subroutineName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void CompileCodeForRunCounterStorageDeclarations(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileDeclaration(writer);
        }

        public void CompileCodeForRunCounterStorageInitialization(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileInitialization(writer);
        }

        public void CompileCodeForThreads(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileThreadCode(writer);
        }

        public void CompileCodeForThreadPrograms(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileProgram(writer);
        }

        public void CompileCodeForMainProgram(TextWriter writer)
        {
            using (StringWriter mainProgram = new StringWriter())
            {
                parser.ParseTree.Where(s => !(s is SubroutineStatement)).Compile(mainProgram, true);
                CompileCodeForThreadDispatchTable(writer);
                writer.Write(mainProgram);
            }
        }

        private void CompileCodeForThreadDispatchTable(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileDispatchTable(writer);
        }

        public void CompileCodeForSubroutines(TextWriter writer, EV3CompilerContext context)
        {
            foreach (EV3SubDefinition sub in subroutines.Where(s => s.IsReferenced))
            {
                writer.WriteLine();
                sub.Compile(writer, context, new string[0], null);
            }
        }
    }
}

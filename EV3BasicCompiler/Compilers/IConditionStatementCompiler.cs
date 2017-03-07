using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public interface IConditionStatementCompiler
    {
        IBooleanExpressionCompiler ConditionCompiler { get; }
        bool IsAlwaysFalse { get; }
        bool IsAlwaysTrue { get; }
        void CompileStatements(TextWriter writer, bool isRootStatement);
    }
}

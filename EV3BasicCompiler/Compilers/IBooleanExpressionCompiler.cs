using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public interface IBooleanExpressionCompiler : IExpressionCompiler
    {
        void CompileBranch(TextWriter writer, string label);
        void CompileBranchNegated(TextWriter writer, string label);
    }
}

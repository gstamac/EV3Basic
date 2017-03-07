using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public interface IAssignmentExpressionCompiler : IExpressionCompiler
    {
        void CompileAssignment(TextWriter writer, string compiledValue, string outputName);
        string VariableName { get; }
        EV3Variable Variable { get; }
        int Index { get; }
        bool IsArray { get; }
    }
}

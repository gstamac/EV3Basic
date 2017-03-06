using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public interface IExpressionCompiler 
    {
        EV3Type Type { get; }
        bool IsLiteral { get; }
        string Value { get; }
        bool CanCompileBoolean { get; }
        bool BooleanValue { get; }
        EV3CompilerContext Context { get; }
        string Compile(TextWriter writer, IEV3Variable variable);
 }
}

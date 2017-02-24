namespace EV3BasicCompiler.Compilers
{
    public interface IExpressionCompiler 
    {
        EV3Type Type { get; }
        bool IsLiteral { get; }
        string Value { get; }
        EV3CompilerContext Context { get; }
    }
}

namespace EV3BasicCompiler
{
    public interface IEV3Variable
    {
        string Ev3Name { get; }
        EV3Type Type { get; }
    }
}

using System;
using System.Linq;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public interface IStatementCompiler
    {
        void Compile(TextWriter writer, bool isRootStatement);
    }
}

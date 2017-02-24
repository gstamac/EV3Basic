using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class SubroutinesTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldDefineEmptySub()
        {
            TestIt(@"
                Sub Sub1
                EndSub
            ", @"
                SUB_SUB1:
                    SUB8 STACKPOINTER 1 STACKPOINTER
                    READ32 RETURNSTACK STACKPOINTER INDEX
                    JR_DYNAMIC INDEX
                ENDSUB_SUB1:
            ", ExtractSubroutinesCode);
        }

    }
}

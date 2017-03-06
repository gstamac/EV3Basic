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

        [TestMethod]
        public void ShouldCompileExecutingSub()
        {
            TestIt(@"
                TestSub()
                Sub TestSub
                EndSub
            ", @"
            subcall PROGRAM_MAIN
            {
                IN_32 SUBPROGRAM
                DATA32 INDEX
                ARRAY8 STACKPOINTER 4
                ARRAY32 RETURNSTACK2 128
                ARRAY32 RETURNSTACK 128
                MOVE8_8 0 STACKPOINTER
                WRITE32 ENDSUB_TESTSUB:CALLSUB0 STACKPOINTER RETURNSTACK
                ADD8 STACKPOINTER 1 STACKPOINTER
                JR SUB_TESTSUB
            CALLSUB0:
            ENDTHREAD:
                RETURN
            }
            ", ExtractFullMainProgramCode);
        }
    }
}

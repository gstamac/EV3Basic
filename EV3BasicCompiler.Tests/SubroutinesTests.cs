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
                Sub1()
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
        public void ShouldNotDefineSub_WhenSubIsNotCalled()
        {
            TestIt(@"
                Sub Sub1
                EndSub
            ", @"
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

        [TestMethod]
        public void ShouldDeclareFloatVariableInSubroutine()
        {
            TestDeclaration(@"
                SUB1()
                Sub SUB1
                    j = 10
                EndSub
            ", @"
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareStringVariableInSubroutine()
        {
            TestDeclaration(@"
                SUB1()
                Sub SUB1
                    j = ""X""
                EndSub
            ", @"
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareArrayVariableInSubroutine()
        {
            TestDeclaration(@"
                SUB1()
                Sub SUB1
                    j[10] = 10
                EndSub
            ", @"
                ARRAY16 VJ 10
            ");
        }

        [TestMethod]
        public void ShouldNotDeclareFloatVariableInSubroutine_WhenSubroutineNotUsed()
        {
            TestDeclaration(@"
                Sub SUB1
                    j = 10
                EndSub
            ", @"
            ");
        }

        [TestMethod]
        public void ShouldNotDeclareStringVariableInSubroutine_WhenSubroutineNotUsed()
        {
            TestDeclaration(@"
                Sub SUB1
                    j = ""X""
                EndSub
            ", @"
            ");
        }

        [TestMethod]
        public void ShouldNotDeclareArrayVariableInSubroutine_WhenSubroutineNotUsed()
        {
            TestDeclaration(@"
                Sub SUB1
                    j[10] = 10
                EndSub
            ", @"
            ");
        }

        [TestMethod]
        public void ShouldUseGlobalFloatVariableInSubroutine()
        {
            TestIt(@"
                j = EV3.Time
                Sub SUB1
                    A = j + 10
                EndSub
            ", @"
            ", ExtractSubroutinesCode);
        }

        [TestMethod]
        public void ShouldUseGlobalStringVariableInSubroutine()
        {
            TestIt(@"
                j = Buttons.Current
                Sub SUB1
                    A = j + ""X""
                EndSub
            ", @"
            ", ExtractSubroutinesCode);
        }

        [TestMethod]
        public void ShouldUseGlobalArrayVariableInSubroutine()
        {
            TestIt(@"
                j = Vector.Init(5, 141)
                Sub SUB1
                    A = Vector.Sort(5, j)
                EndSub
            ", @"
            ", ExtractSubroutinesCode);
        }

        [TestMethod]
        public void ShouldProcessRecursiveSubroutines()
        {
            TestDeclaration(@"
                SUB1()
                Sub SUB1
                    j = 10
                    SUB1()
                EndSub
            ", @"
                DATAF VJ
            ");
        }

    }
}

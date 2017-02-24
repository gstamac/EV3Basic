using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class ThreadTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldDeclareThread()
        {
            TestDeclaration(@"
                Thread.Run = THREAD1

                Sub THREAD1
                EndSub
            ", @"
                DATA32 RUNCOUNTER_THREAD1
            ");
        }

        [TestMethod]
        public void ShouldInitializeThread()
        {
            TestInitialization(@"
                Thread.Run = THREAD1

                Sub THREAD1
                EndSub
            ", @"
                MOVE32_32 0 RUNCOUNTER_THREAD1
            ");
        }

        [TestMethod]
        public void ShouldCompileThreadCode()
        {
            TestIt(@"
                Thread.Run = THREADA

                Sub THREADA
                EndSub
            ", @"
                vmthread TTHREADA
                {
                    DATA32 tmp
                  launch:
                    CALL PROGRAM_THREADA 0
                    CALL GETANDINC32 RUNCOUNTER_THREADA -1 RUNCOUNTER_THREADA tmp
                    JR_GT32 tmp 1 launch
                }
            ", ExtractThreadCode);
        }

        [TestMethod]
        public void ShouldCompileMultipleThreadCode()
        {
            TestIt(@"
                Thread.Run = THREADA
                Thread.Run = THREADB

                Sub THREADA
                EndSub

                Sub THREADB
                EndSub
            ", @"
                vmthread TTHREADA
                {
                    DATA32 tmp
                  launch:
                    CALL PROGRAM_THREADA 0
                    CALL GETANDINC32 RUNCOUNTER_THREADA -1 RUNCOUNTER_THREADA tmp
                    JR_GT32 tmp 1 launch
                }
                vmthread TTHREADB
                {
                    DATA32 tmp
                  launch:
                    CALL PROGRAM_THREADB 1
                    CALL GETANDINC32 RUNCOUNTER_THREADB -1 RUNCOUNTER_THREADB tmp
                    JR_GT32 tmp 1 launch
                }
            ", ExtractThreadCode);
        }

        [TestMethod]
        public void ShouldCompileThreadProgramCode()
        {
            TestIt(@"
                Thread.Run = THREADA
                Thread.Run = THREADB

                Sub THREADA
                EndSub

                Sub THREADB
                EndSub
            ", @"
                subcall PROGRAM_MAIN
                subcall PROGRAM_THREADA
                subcall PROGRAM_THREADB
                {
                    IN_32 SUBPROGRAM
                    DATA32 INDEX
                    ARRAY8 STACKPOINTER 4
                    ARRAY32 RETURNSTACK2 128
                    ARRAY32 RETURNSTACK 128

                    MOVE8_8 0 STACKPOINTER
                    JR_NEQ32 SUBPROGRAM 0 dispatch2
                    WRITE32 ENDSUB_THREADA:ENDTHREAD STACKPOINTER RETURNSTACK
                    ADD8 STACKPOINTER 1 STACKPOINTER
                    JR SUB_THREADA
                  dispatch2:
                    JR_NEQ32 SUBPROGRAM 1 dispatch3
                    WRITE32 ENDSUB_THREADB:ENDTHREAD STACKPOINTER RETURNSTACK
                    ADD8 STACKPOINTER 1 STACKPOINTER
                    JR SUB_THREADB
                  dispatch3:
                    DATA32 tmp0
                    CALL GETANDINC32 RUNCOUNTER_THREADA 1  RUNCOUNTER_THREADA tmp0
                    JR_NEQ32 0 tmp0 alreadylaunched0
                    OBJECT_START TTHREADA
                  alreadylaunched0:
                    DATA32 tmp1
                    CALL GETANDINC32 RUNCOUNTER_THREADB 1  RUNCOUNTER_THREADB tmp1
                    JR_NEQ32 0 tmp1 alreadylaunched1
                    OBJECT_START TTHREADB
                  alreadylaunched1:
                ENDTHREAD:
                    RETURN
                }
            ", ExtractFullMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileThreadSubroutines()
        {
            TestIt(@"
                Thread.Run = THREADA
                Thread.Run = THREADB

                Sub THREADA
                EndSub

                Sub THREADB
                EndSub
            ", @"
                SUB_THREADA:
                    SUB8 STACKPOINTER 1 STACKPOINTER
                    READ32 RETURNSTACK STACKPOINTER INDEX
                    JR_DYNAMIC INDEX
                ENDSUB_THREADA:
                SUB_THREADB:
                    SUB8 STACKPOINTER 1 STACKPOINTER
                    READ32 RETURNSTACK STACKPOINTER INDEX
                    JR_DYNAMIC INDEX
                ENDSUB_THREADB:
            ", ExtractSubroutinesCode);
        }

        private string ExtractThreadCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, "vmthread MAIN[^{]*{[^}]*}(.*)subcall PROGRAM_MAIN", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return ev3Code;
        }
    }
}

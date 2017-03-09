using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class MainProgramTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileEmptyProgramFull()
        {
            TestFull("", @"
                DATA16 FD_NATIVECODECOMMAND
                DATA16 FD_NATIVECODERESPONSE
                DATA32 STOPLCDUPDATE
                DATA32 NUMMAILBOXES
                ARRAY16 LOCKS 2 

                vmthread MAIN
                {
                    MOVE32_32 0 STOPLCDUPDATE
                    MOVE32_32 0 NUMMAILBOXES
                    OUTPUT_RESET 0 15
                    INPUT_DEVICE CLR_ALL -1
                    ARRAY CREATE8 0 LOCKS

                    ARRAY CREATE8 1 LOCKS
                    CALL PROGRAM_MAIN -1
                    PROGRAM_STOP -1
                }

                subcall PROGRAM_MAIN
                {
                    IN_32 SUBPROGRAM
                    DATA32 INDEX
                    ARRAY8 STACKPOINTER 4
                    ARRAY32 RETURNSTACK2 128
                    ARRAY32 RETURNSTACK 128
                    MOVE8_8 0 STACKPOINTER

                    ENDTHREAD:
                        RETURN
                }
            ");
        }

        [TestMethod]
        public void ShouldCompileEmptyProgram()
        {
            TestIt("", @"
                subcall PROGRAM_MAIN
                {
                    IN_32 SUBPROGRAM
                    DATA32 INDEX
                    ARRAY8 STACKPOINTER 4
                    ARRAY32 RETURNSTACK2 128
                    ARRAY32 RETURNSTACK 128
                    MOVE8_8 0 STACKPOINTER

                    ENDTHREAD:
                        RETURN
                }
            ", ExtractFullMainProgramCode);
        }

        [TestMethod]
        public void ShouldProvideCorrectErrorLineAndColumn()
        {
            TestCompileFailure($@"
                If ""X"" = 1 Then
                EndIf
            ", "Boolean operations on unrelated types are not permited", 2, 20);
        }
    }
}

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class LibraryTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void Test()
        {
            TestIt("", @"
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
    }
}

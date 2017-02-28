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
        public void ShouldExecuteExternalMethod() 
        {
            TestIt(@"
                LCD.Clear()
            ", @"
                CALL LCD.CLEAR
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldExecuteExternalFunction_WhenReturnTypeIsFloat()
        {
            TestIt(@"
                EV3.BatteryLevel()
            ", @"
                CALL EV3.BATTERYLEVEL F0
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldExecuteExternalFunction_WhenReturnTypeIsString()
        {
            TestIt(@"
                Mailbox.IsAvailable(1)
            ", @"
                CALL MAILBOX.ISAVAILABLE 1.0 S0
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldExecuteExternalFunction_WhenReturnTypeIsArray() // NEW!!!!!
        {
            TestIt(@"
                Sensor.ReadRaw(1, 8)
            ", @"
                CALL SENSOR.READRAW 1.0 8.0 A0
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldExecuteExternalInlineMethod()
        {
            TestIt(@"
                Buttons.Wait()
            ", @"
                UI_BUTTON WAIT_FOR_PRESS                         
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldExecuteExternalInlineFunction_WhenReturnTypeIsString()
        {
            TestIt(@"
                Mailbox.Receive(8)
            ", @"
                DATA8 no0                  
                MOVEF_8 8.0 no0            
                MAILBOX_READY no0          
                MAILBOX_READ no0 252 1 S0                            
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldFailExecutingExternalMethod_WhenNotEnoughParameters()
        {
            TestCompileFailure(@"
                Sensor.ReadRaw(1)
            ", "Incorrect number of parameters. Expected 2.", 2, 17);
        }

        [TestMethod]
        public void ShouldFailExecutingExternalMethod_WhenTooManyParameters()
        {
            TestCompileFailure(@"
                Sensor.ReadRaw(1, 3, 5)
            ", "Incorrect number of parameters. Expected 2.", 2, 17);
        }

        [TestMethod]
        public void ShouldFailExecutingExternalInlineMethod_WhenNotEnoughParameters()
        {
            TestCompileFailure(@"
                Mailbox.Receive()
            ", "Incorrect number of parameters. Expected 1.", 2, 17);
        }

        [TestMethod]
        public void ShouldFailExecutingExternalInlineMethod_WhenTooManyParameters()
        {
            TestCompileFailure(@"
                Mailbox.Receive(1, 3)
            ", "Incorrect number of parameters. Expected 1.", 2, 17);
        }

    }
}

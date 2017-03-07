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
        public void ShouldExecuteExternalOnelineInlineMethod_WithParameters()
        {
            TestIt(@"
                Math.Floor(1.3)
            ", @"
                MATH FLOOR 1.3 F0                        
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
        public void ShouldConvertParameters_WhenExecutingInlineMethod()
        {
            TestIt(@"
                Motor.Wait(EV3.BatteryLevel)
            ", @"
                CALL EV3.BATTERYLEVEL F0
                STRINGS VALUE_FORMATTED F0 '%g' 99 S0
                DATA8 layer0
                DATA8 nos0
                DATA8 busy0
                CALL MOTORDECODEPORTSDESCRIPTOR S0 layer0 nos0
              motorwaiting0:
                OUTPUT_TEST layer0 nos0 busy0
                JR_EQ8 busy0 0 motornotbusy0
                SLEEP
                JR motorwaiting0
              motornotbusy0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldConvertParameters_WhenExecutinginlineMethodWithConstant()
        {
            TestIt(@"
                Motor.Wait(1)
            ", @"
                DATA8 layer0
                DATA8 nos0
                DATA8 busy0
                CALL MOTORDECODEPORTSDESCRIPTOR '1.0' layer0 nos0
              motorwaiting0:
                OUTPUT_TEST layer0 nos0 busy0
                JR_EQ8 busy0 0 motornotbusy0
                SLEEP
                JR motorwaiting0
              motornotbusy0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldConvertParameters_WhenExecutingExternalMethod()
        {
            TestIt(@"
                LCD.Text(1, 50, 50, 2, EV3.BatteryLevel)
            ", @"
                CALL EV3.BATTERYLEVEL F0             
                STRINGS VALUE_FORMATTED F0 '%g' 99 S0
                CALL LCD.TEXT 1.0 50.0 50.0 2.0 S0   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldConvertParameters_WhenExecutingExternalMethodWithConstant()
        {
            TestIt(@"
                LCD.Text(1, 50, 50, 2, 10)
            ", @"
                CALL LCD.TEXT 1.0 50.0 50.0 2.0 '10.0'
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldIgnoreTextWindowMethods()  // NEW!!!!!
        {
            TestIt(@"
                TextWindow.WriteLine(""X"")
            ", @"
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldFailExecutingExternalMethod_WhenNotEnoughParameters()
        {
            TestCompileFailure(@"
                Sensor.ReadRaw(1)
            ", "Incorrect number of parameters. Expected 2.");
        }

        [TestMethod]
        public void ShouldFailExecutingExternalMethod_WhenTooManyParameters()
        {
            TestCompileFailure(@"
                Sensor.ReadRaw(1, 3, 5)
            ", "Incorrect number of parameters. Expected 2.");
        }

        [TestMethod]
        public void ShouldFailExecutingExternalInlineMethod_WhenNotEnoughParameters()
        {
            TestCompileFailure(@"
                Mailbox.Receive()
            ", "Incorrect number of parameters. Expected 1.");
        }

        [TestMethod]
        public void ShouldFailExecutingExternalInlineMethod_WhenTooManyParameters()
        {
            TestCompileFailure(@"
                Mailbox.Receive(1, 3)
            ", "Incorrect number of parameters. Expected 1.");
        }

        [TestMethod]
        public void ShouldOutputReferencedSubroutines()
        {
            TestIt(@"
                EV3.Time()
            ", @"
                subcall EV3.TIME  // F
            ", ExtractReferenceDefinitionsCode);
        }

        [TestMethod]
        public void ShouldOutputMultipleReferencedSubroutines()
        {
            TestIt(@"
                EV3.Time()
                LCD.Clear()
            ", @"
                subcall EV3.TIME
                subcall LCD.CLEAR
            ", ExtractReferenceDefinitionsCode);
        }

        [TestMethod]
        public void ShouldOutputReferencedSubroutines_WhenReferencedFromSubroutine()
        {
            TestIt(@"
                Motor.Stop(""1"", ""2"")
            ", @"
                subcall MOTOR.STOP
                subcall MOTORDECODEPORTSDESCRIPTOR
            ", ExtractReferenceDefinitionsCode);
        }

        [TestMethod]
        public void ShouldOutputReferencedSubroutines_WhenReferencedFromMultiLevels()
        {
            TestIt(@"
                Assert.Near(1, 2, ""X"")
            ", @"
                subcall ASSERT.FAILED
                subcall ASSERT.NEAR
                subcall TEXT.GETSUBTEXT
                subcall TEXT.GETSUBTEXTTOEND
            ", ExtractReferenceDefinitionsCode);
        }

        [TestMethod]
        public void ShouldOutputReferencedSubroutines_WhenReferencedFromInline()
        {
            TestIt(@"
                Thread.Lock(1)
            ", @"
                subcall GETANDSETLOCK
            ", ExtractReferenceDefinitionsCode);
        }

        [TestMethod]
        public void Test()
        {
            TestIt(@"
                raw = Sensor.ReadRaw(1,8)
                For i=0 To 3
                    LCD.Write(0, 64 + i * 16, ""RAW"" + i + "":"" + raw[i])
                EndFor
            ", @"
                CALL SENSOR.READRAW 1.0 8.0 VRAW
                MOVEF_F 0.0 VI
              for1:
                JR_GTF VI 3.0 endfor1
              forbody1:
                MULF VI 16.0 F0
                ADDF 64.0 F0 F1
                STRINGS VALUE_FORMATTED VI '%g' 99 S0
                CALL TEXT.APPEND 'RAW' S0 S1
                CALL TEXT.APPEND S1 ':' S0
                CALL ARRAYGET_FLOAT VI F0 VRAW
                STRINGS VALUE_FORMATTED F0 '%g' 99 S1
                CALL TEXT.APPEND S0 S1 S2
                CALL LCD.WRITE 0.0 F1 S2
                ADDF VI 1.0 VI
                JR_LTEQF VI 3.0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }
    }
}

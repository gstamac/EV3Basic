using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class LoopsTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileForLoop()
        {
            TestIt(@"
                For i = 10 To 24
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 10.0 VI              
              for0:                        
                JR_GTF VI 24.0 endfor0       
              forbody0:          
                UI_BUTTON WAIT_FOR_PRESS          
                ADDF VI 1.0 VI               
                JR_LTEQF VI 24.0 forbody0    
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenFromContainsFormula()
        {
            TestIt(@"
                j = 0
                For i = j + 3 To 24
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 0.0 VJ
                ADDF VJ 3.0 VI
              for0:
                JR_GTF VI 24.0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                JR_LTEQF VI 24.0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenFromContainsReference()
        {
            TestIt(@"
                j = 0
                For i = j To 24
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 0.0 VJ
                MOVEF_F VJ VI
              for0:
                JR_GTF VI 24.0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                JR_LTEQF VI 24.0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenFromContainsReferenceToArray()
        {
            TestIt(@"
                j[1] = 0
                For i = j[1] To 24
                    Buttons.Wait()
                EndFor
            ", @"
                CALL ARRAYSTORE_FLOAT 1.0 0.0 VJ
                CALL ARRAYGET_FLOAT 1.0 VI VJ
              for1:
                JR_GTF VI 24.0 endfor1
              forbody1:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                JR_LTEQF VI 24.0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenToContainsFormula()
        {
            TestIt(@"
                j = 1
                For i = 10 To j * 20
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                MULF VJ 20.0 F0
                JR_GTF VI F0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                MULF VJ 20.0 F0
                JR_LTEQF VI F0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenToContainsReference()
        {
            TestIt(@"
                j = 1
                For i = 10 To j
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                JR_GTF VI VJ endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                JR_LTEQF VI VJ forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoop_WhenToContainsReferenceToArray()
        {
            TestIt(@"
                j[1] = 1
                For i = 10 To j[1]
                    Buttons.Wait()
                EndFor
            ", @"
                CALL ARRAYSTORE_FLOAT 1.0 1.0 VJ
                MOVEF_F 10.0 VI
              for1:
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                JR_GTF VI F0 endfor1
              forbody1:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 1.0 VI
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                JR_LTEQF VI F0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep()
        {
            TestIt(@"
                For i = 10 To 24 Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 10.0 VI              
              for0:                        
                JR_GTF VI 24.0 endfor0       
              forbody0:                    
                UI_BUTTON WAIT_FOR_PRESS          
                ADDF VI 2.0 VI               
                JR_LTEQF VI 24.0 forbody0    
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithNegativeStep()
        {
            TestIt(@"
                For i = 10 To 0 Step -2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 10.0 VI
              for0:
                JR_LTF VI 0.0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI -2.0 VI
                JR_GTEQF VI 0.0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenFromContainsFormula()
        {
            TestIt(@"
                j = 1
                For i = j + 3 To 24 Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                ADDF VJ 3.0 VI
              for0:
                JR_GTF VI 24.0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                JR_LTEQF VI 24.0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenFromContainsReference()
        {
            TestIt(@"
                j = 1
                For i = j To 24 Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F VJ VI
              for0:
                JR_GTF VI 24.0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                JR_LTEQF VI 24.0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenFromContainsReferenceToArray()
        {
            TestIt(@"
                j[1] = 1
                For i = j[1] To 24 Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                CALL ARRAYSTORE_FLOAT 1.0 1.0 VJ
                CALL ARRAYGET_FLOAT 1.0 VI VJ
              for1:
                JR_GTF VI 24.0 endfor1
              forbody1:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                JR_LTEQF VI 24.0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenToContainsFormula()
        {
            TestIt(@"
                j = 1
                For i = 10 To j - 32 Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                SUBF VJ 32.0 F0
                JR_GTF VI F0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                SUBF VJ 32.0 F0
                JR_LTEQF VI F0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenToContainsReference()
        {
            TestIt(@"
                j = 1
                For i = 10 To j Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                JR_GTF VI VJ endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                JR_LTEQF VI VJ forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenToContainsReferenceToArray()
        {
            TestIt(@"
                j[1] = 1
                For i = 10 To j[1] Step 2
                    Buttons.Wait()
                EndFor
            ", @"
                CALL ARRAYSTORE_FLOAT 1.0 1.0 VJ
                MOVEF_F 10.0 VI
              for1:
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                JR_GTF VI F0 endfor1
              forbody1:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI 2.0 VI
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                JR_LTEQF VI F0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenStepContainsFormula()      // NEW!!!!! removed redundand step calculation, optimized LE_STEP check
        {
            TestIt(@"
                j = 1
                For i = 10 To 24 Step j + 2
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                ADDF VJ 2.0 F0
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_EQ8 F1 0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VJ 2.0 F0
                ADDF VI F0 VI
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_NEQ8 F1 0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
            // PREVIOUS
            //    MOVEF_F 1.0 VJ                         
            //    MOVEF_F 10.0 VI                        
            //for0:                                    
            //    ADDF VJ 2.0 F0                         
            //    CALL LE_STEP VI 24.0 F0 S0             
            //    AND8888_32 S0 -538976289 S0            
            //    STRINGS COMPARE S0 'TRUE' S0
            //    JR_EQ8 S0 0 endfor0
            //forbody0:                                
            //    UI_BUTTON WAIT_FOR_PRESS               
            //    ADDF VJ 2.0 F0                         
            //    ADDF VI F0 VI                          
            //    ADDF VJ 2.0 F0                         
            //    CALL LE_STEP VI 24.0 F0 S0             
            //    AND8888_32 S0 -538976289 S0
            //    STRINGS COMPARE S0 'TRUE' S0
            //    JR_NEQ8 S0 0 forbody0
            //endfor0:                                                                
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenStepContainsReference()
        {
            TestIt(@"
                j = 1
                For i = 10 To 24 Step j
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                CALL LE_STEP8 VI 24.0 VJ F0
                JR_EQ8 F0 0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                ADDF VI VJ VI
                CALL LE_STEP8 VI 24.0 VJ F0
                JR_NEQ8 F0 0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenStepContainsNegativeReference() // NEW!!!!! removed redundand step calculation, optimized LE_STEP check
        {
            TestIt(@"
                j = 1
                For i = 10 To 24 Step -j
                    Buttons.Wait()
                EndFor
            ", @"
                MOVEF_F 1.0 VJ
                MOVEF_F 10.0 VI
              for0:
                MATH NEGATE VJ F0
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_EQ8 F1 0 endfor0
              forbody0:
                UI_BUTTON WAIT_FOR_PRESS
                MATH NEGATE VJ F0
                ADDF VI F0 VI
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_NEQ8 F1 0 forbody0
              endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep_WhenStepContainsReferenceToArray()     // NEW!!!!! removed redundand step calculation, optimized LE_STEP check
        {
            TestIt(@"
                j[1] = 1
                For i = 10 To 24 Step j[1]
                    Buttons.Wait()
                EndFor
            ", @"
                CALL ARRAYSTORE_FLOAT 1.0 1.0 VJ
                MOVEF_F 10.0 VI
              for1:
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_EQ8 F1 0 endfor1
              forbody1:
                UI_BUTTON WAIT_FOR_PRESS
                CALL ARRAYGET_FLOAT 1.0 F0 VJ
                ADDF VI F0 VI
                CALL LE_STEP8 VI 24.0 F0 F1
                JR_NEQ8 F1 0 forbody1
              endfor1:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileEndlessWhileLoop()
        {
            TestIt(@"
                While (""true"")
                    Buttons.Wait()
                EndWhile
            ", @"
              while0:          
              whilebody0:      
                UI_BUTTON WAIT_FOR_PRESS          
                JR whilebody0    
              endwhile0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileWhileLoop()
        {
            TestIt(@"
                i = 10
                While (i > 0)
                    Buttons.Wait()
                EndWhile
            ", @"
                MOVEF_F 10.0 VI              
              while0:                      
                JR_LTEQF VI 0.0 endwhile0    
              whilebody0:                  
                UI_BUTTON WAIT_FOR_PRESS          
                JR_GTF VI 0.0 whilebody0     
              endwhile0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void IgnoreEmptyLoops()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        [Ignore]
        public void GotoStatemementTests()
        {
            Assert.IsTrue(false);
        }
    }
}

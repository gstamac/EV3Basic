using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class ElseTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsConstantStringFalse()
        {
            TestIt(@"
                If ""false"" Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsAlwaysTrueComparingFloats()
        {
            TestIt(@"
                If 2 = 1 Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsAlwaysTrue_ComparingStrings()
        {
            TestIt(@"
                If ""Y"" = ""X"" Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareEq()
        {
            TestItFloat("i = 1", @"
                JR_NEQF VI 1.0 else0_1     
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareNotEq()
        {
            TestItFloat("i <> 1", @"
                JR_EQF VI 1.0 else0_1          
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareGt()
        {
            TestItFloat("i > 1", @"
                JR_LTEQF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareGtEq()
        {
            TestItFloat("i >= 1", @"
                JR_LTF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLt()
        {
            TestItFloat("i < 1", @"
                JR_GTEQF VI 1.0 else0_1                 
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLtEq()
        {
            TestItFloat("i <= 1", @"
                JR_GTF VI 1.0 else0_1               
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalOr()
        {
            TestItFloat("(i = 1) Or (i > 2)", @"
                JR_EQF VI 1.0 or1               
                JR_LTEQF VI 2.0 else0_1          
              or1:                                                
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalOrOr()
        {
            TestItFloat("(i = 1) Or (i = 2) Or (i = 3)", @"
                JR_EQF VI 1.0 or1                  
                JR_EQF VI 2.0 or1                  
                JR_NEQF VI 3.0 else0_1             
              or1:                                      
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalOrOrOrOr()
        {
            TestItFloat("(i = 1) Or (i = 2) Or (i = 3) Or (i = 4) Or (i = 5)", @"
                JR_EQF VI 1.0 or1         
                JR_EQF VI 2.0 or1         
                JR_EQF VI 3.0 or1         
                JR_EQF VI 4.0 or1         
                JR_NEQF VI 5.0 else0_1    
              or1:                        
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalAnd()
        {
            TestItFloat("(i <= 1) And (i > 2)", @"
                JR_GTF VI 1.0 else0_1      
                JR_LTEQF VI 2.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalAndAnd()
        {
            TestItFloat("(i <= 1) And (i > 2) And (i <> 3)", @"
                JR_GTF VI 1.0 else0_1          
                JR_LTEQF VI 2.0 else0_1        
                JR_EQF VI 3.0 else0_1                              
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalAndAndAndAnd()
        {
            TestItFloat("(i = 1) And (i = 2) And (i = 3) And (i = 4) And (i = 5)", @"
                JR_NEQF VI 1.0 else0_1   
                JR_NEQF VI 2.0 else0_1   
                JR_NEQF VI 3.0 else0_1   
                JR_NEQF VI 4.0 else0_1   
                JR_NEQF VI 5.0 else0_1          
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalAndOrPrecedence()
        {
            //  == ((i <= 1) And (i > 2)) Or (3 < i)
            TestItFloat("(i <= 1) And (i > 2) Or (3 < i)", @"
                JR_GTF VI 1.0 and2        
                JR_GTF VI 2.0 or1         
              and2:                       
                JR_GTEQF 3.0 VI else0_1   
              or1:                                
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalOrAndPrecedence()
        {
            //  == (i <= 1) Or ((i > 2) And (3 < i))
            TestItFloat("(i = 1) Or (i > 2) And (3 < i)", @"
                JR_EQF VI 1.0 or1           
                JR_LTEQF VI 2.0 else0_1      
                JR_GTEQF 3.0 VI else0_1      
              or1:                               
            ");
        }

        [TestMethod]
        public void ShouldCompileElse_WhenConditionIsFloatCompareLogicalComplicated()
        {
            TestItFloat("(i <= 1) And ((i > 2) And (3 < i)) Or ((i > 4) And (i = 5))", @"
                JR_GTF VI 1.0 and2       
                JR_LTEQF VI 2.0 and3     
                JR_LTF 3.0 VI or1        
              and3:                      
              and2:                      
                JR_LTEQF VI 4.0 else0_1  
                JR_NEQF VI 5.0 else0_1                                              
              or1:                      
            ");
        }

        private void TestItFloat(string condition, string branchCode)
        {
            TestIt(@"
                i = 2
                If " + condition + @" Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                MOVEF_F 2.0 VI          
            " + branchCode + @"
                UI_BUTTON FLUSH
                JR endif0
              else0_1:
                UI_BUTTON WAIT_FOR_PRESS
              else0_2:                  
              endif0:   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileMultipleIfsWithElse()      // NEW!!!!! optimized label numbering
        {
            TestIt(@"
                i = 2
                If (i = 1) Or (i = 2) Then
                    Buttons.Flush()
                Else
                    LCD.StopUpdate()
                EndIf
                If (i = 3) Or (i = 4) Then
                    Buttons.Wait()
                Else
                    Buttons.Flush()
                EndIf
                If (i = 5) And (i = 6) Then
                    LCD.StopUpdate()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                MOVEF_F 2.0 VI                 
                JR_EQF VI 1.0 or1              
                JR_NEQF VI 2.0 else0_1         
              or1:                             
                UI_BUTTON FLUSH                
                JR endif0                      
              else0_1:                         
                MOVE32_32 1 STOPLCDUPDATE      
              else0_2:                         
              endif0:                          
                JR_EQF VI 3.0 or3              
                JR_NEQF VI 4.0 else2_1         
              or3:                             
                UI_BUTTON WAIT_FOR_PRESS       
                JR endif2                      
              else2_1:                         
                UI_BUTTON FLUSH                
              else2_2:                         
              endif2:                                                                        
                JR_NEQF VI 5.0 else4_1         
                JR_NEQF VI 6.0 else4_1         
                MOVE32_32 1 STOPLCDUPDATE      
                JR endif4                      
              else4_1:                         
                UI_BUTTON WAIT_FOR_PRESS       
              else4_2:                         
              endif4:                                         
            ", ExtractMainProgramCode);
        }


    }
}

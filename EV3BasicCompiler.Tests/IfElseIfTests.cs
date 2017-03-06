using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class IfElseIfTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionIsLiteralFalseAndElseIfConditionIsLiteralTrue()
        {
            TestIt(@"
                If ""false"" Then
                    Buttons.Flush()
                ElseIf ""true"" Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionIsLiteralTrueAndElseIfConditionIsLiteralFalse()
        {
            TestIt(@"
                If ""true"" Then
                    Buttons.Wait()
                ElseIf ""false"" Then
                    Buttons.Flush()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsAlwaysTrueComparingFloats()
        {
            TestIt(@"
                If 2 = 1 Then
                    Buttons.Flush()
                ElseIf 2 = 2 Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsAlwaysTrue_ComparingStrings()
        {
            TestIt(@"
                If ""Y"" = ""X"" Then
                    Buttons.Flush()
                ElseIf ""Y"" = ""Y"" Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionsIsLiteralFalseAndElseIfConditionIsNonliteral()
        {
            TestIt(@"
                i = 0
                If ""false"" Then
                    Buttons.Flush()
                ElseIf i > 0 Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 0.0 VI           
                JR_LTEQF VI 0.0 else0_1  
                UI_BUTTON WAIT_FOR_PRESS 
                JR endif0                
              else0_1:                   
                MOVE32_32 1 STOPLCDUPDATE
              else0_2:                   
              endif0:                    
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionsIsLiteralTrueAndElseIfConditionIsNonliteral()
        {
            TestIt(@"
                i = 0
                If ""true"" Then
                    Buttons.Flush()
                ElseIf i > 0 Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 0.0 VI
                UI_BUTTON FLUSH
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionsIsNonliteralAndElseIfConditionIsLiteralTrue()
        {
            TestIt(@"
                i = 0
                If i > 0 Then
                    Buttons.Flush()
                ElseIf ""true"" Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 0.0 VI          
                JR_LTEQF VI 0.0 else0_1 
                UI_BUTTON FLUSH         
                JR endif0               
              else0_1:                  
                UI_BUTTON WAIT_FOR_PRESS
              else0_2:                  
              endif0:                   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenIfConditionsIsNonliteralAndElseIfConditionIsLiteralFalse()
        {
            TestIt(@"
                i = 0
                If i > 0 Then
                    Buttons.Flush()
                ElseIf ""false"" Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 0.0 VI           
                JR_LTEQF VI 0.0 else0_1  
                UI_BUTTON FLUSH          
                JR endif0                
              else0_1:                   
                MOVE32_32 1 STOPLCDUPDATE
              else0_2:                   
              endif0:                    
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileMultipleElseIfs()
        {
            TestIt(@"
                i = 0
                If i = 1 Then
                    Buttons.Flush()
                ElseIf i = 2 Then
                    Buttons.Wait()
                ElseIf i = 3 Then
                    Buttons.Flush()
                ElseIf i = 4 Then
                    LCD.StopUpdate()
                ElseIf i = 5 Then
                    Buttons.Wait()
                Else
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 0.0 VI           
                JR_NEQF VI 1.0 else0_1   
                UI_BUTTON FLUSH          
                JR endif0                
              else0_1:                   
                JR_NEQF VI 2.0 else0_2   
                UI_BUTTON WAIT_FOR_PRESS 
                JR endif0                
              else0_2:                   
                JR_NEQF VI 3.0 else0_3   
                UI_BUTTON FLUSH          
                JR endif0                
              else0_3:                   
                JR_NEQF VI 4.0 else0_4   
                MOVE32_32 1 STOPLCDUPDATE
                JR endif0                
              else0_4:                   
                JR_NEQF VI 5.0 else0_5   
                UI_BUTTON WAIT_FOR_PRESS 
                JR endif0                
              else0_5:                   
                MOVE32_32 1 STOPLCDUPDATE
              else0_6:                   
              endif0:                    
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareEq()
        {
            TestItFloat("i = 1", @"
                JR_NEQF VI 0.0 else0_1         
                MOVE32_32 1 STOPLCDUPDATE      
                JR endif0                      
              else0_1:                         
                JR_NEQF VI 1.0 else0_2         
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareNotEq()
        {
            TestItFloat("i <> 1", @"
                JR_NEQF VI 0.0 else0_1                
                MOVE32_32 1 STOPLCDUPDATE             
                JR endif0                             
              else0_1:                                
                JR_EQF VI 1.0 else0_2                         
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareGt()
        {
            TestItFloat("i > 1", @"
                JR_NEQF VI 0.0 else0_1         
                MOVE32_32 1 STOPLCDUPDATE      
                JR endif0                      
              else0_1:                         
                JR_LTEQF VI 1.0 else0_2                      
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareGtEq()
        {
            TestItFloat("i >= 1", @"
                JR_NEQF VI 0.0 else0_1        
                MOVE32_32 1 STOPLCDUPDATE     
                JR endif0                     
              else0_1:                        
                JR_LTF VI 1.0 else0_2              
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLt()
        {
            TestItFloat("i < 1", @"
                JR_NEQF VI 0.0 else0_1                 
                MOVE32_32 1 STOPLCDUPDATE         
                JR endif0                         
              else0_1:                            
                JR_GTEQF VI 1.0 else0_2           
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLtEq()
        {
            TestItFloat("i <= 1", @"
                JR_NEQF VI 0.0 else0_1                    
                MOVE32_32 1 STOPLCDUPDATE           
                JR endif0                           
              else0_1:                              
                JR_GTF VI 1.0 else0_2               
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalOr()
        {
            TestItFloat("(i = 1) Or (i > 2)", @"
                JR_NEQF VI 0.0 else0_1          
                MOVE32_32 1 STOPLCDUPDATE       
                JR endif0                                        
              else0_1:                          
                JR_EQF VI 1.0 or1               
                JR_LTEQF VI 2.0 else0_2         
              or1:                                       
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalOrOr()
        {
            TestItFloat("(i = 1) Or (i = 2) Or (i = 3)", @"
                JR_NEQF VI 0.0 else0_1               
                MOVE32_32 1 STOPLCDUPDATE            
                JR endif0                            
              else0_1:                                    
                JR_EQF VI 1.0 or1                 
                JR_EQF VI 2.0 or1                 
                JR_NEQF VI 3.0 else0_2            
              or1:                                        
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalOrOrOrOr()
        {
            TestItFloat("(i = 1) Or (i = 2) Or (i = 3) Or (i = 4) Or (i = 5)", @"
                JR_NEQF VI 0.0 else0_1         
                MOVE32_32 1 STOPLCDUPDATE      
                JR endif0                      
              else0_1:                         
                JR_EQF VI 1.0 or1              
                JR_EQF VI 2.0 or1              
                JR_EQF VI 3.0 or1              
                JR_EQF VI 4.0 or1              
                JR_NEQF VI 5.0 else0_2         
              or1:                                       
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalAnd()
        {
            TestItFloat("(i <= 1) And (i > 2)", @"
                JR_NEQF VI 0.0 else0_1             
                MOVE32_32 1 STOPLCDUPDATE          
                JR endif0                          
              else0_1:                             
                JR_GTF VI 1.0 else0_2              
                JR_LTEQF VI 2.0 else0_2                      
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalAndAnd()
        {
            TestItFloat("(i <= 1) And (i > 2) And (i <> 3)", @"
                JR_NEQF VI 0.0 else0_1           
                MOVE32_32 1 STOPLCDUPDATE        
                JR endif0                                     
              else0_1:                           
                JR_GTF VI 1.0 else0_2            
                JR_LTEQF VI 2.0 else0_2          
                JR_EQF VI 3.0 else0_2            
            ");                                                                                
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalAndAndAndAnd()
        {
            TestItFloat("(i = 1) And (i = 2) And (i = 3) And (i = 4) And (i = 5)", @"
                JR_NEQF VI 0.0 else0_1           
                MOVE32_32 1 STOPLCDUPDATE        
                JR endif0                        
              else0_1:                           
                JR_NEQF VI 1.0 else0_2           
                JR_NEQF VI 2.0 else0_2           
                JR_NEQF VI 3.0 else0_2           
                JR_NEQF VI 4.0 else0_2           
                JR_NEQF VI 5.0 else0_2           
            ");                                                                               
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalAndOrPrecedence()
        {
            //  == ((i <= 1) And (i > 2)) Or (3 < i)
            TestItFloat("(i <= 1) And (i > 2) Or (3 < i)", @"
                JR_NEQF VI 0.0 else0_1              
                MOVE32_32 1 STOPLCDUPDATE           
                JR endif0                           
              else0_1:                              
                JR_GTF VI 1.0 and2                  
                JR_GTF VI 2.0 or1                   
              and2:                                 
                JR_GTEQF 3.0 VI else0_2             
              or1:                                            
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalOrAndPrecedence()
        {
            //  == (i <= 1) Or ((i > 2) And (3 < i))
            TestItFloat("(i = 1) Or (i > 2) And (3 < i)", @"
                JR_NEQF VI 0.0 else0_1              
                MOVE32_32 1 STOPLCDUPDATE            
                JR endif0                            
              else0_1:                                   
                JR_EQF VI 1.0 or1                  
                JR_LTEQF VI 2.0 else0_2            
                JR_GTEQF 3.0 VI else0_2            
              or1:                                    
            ");
        }

        [TestMethod]
        public void ShouldCompileElseIf_WhenConditionIsFloatCompareLogicalComplicated()
        {
            TestItFloat("(i <= 1) And ((i > 2) And (3 < i)) Or ((i > 4) And (i = 5))", @"
                JR_NEQF VI 0.0 else0_1             
                MOVE32_32 1 STOPLCDUPDATE          
                JR endif0                          
              else0_1:                             
                JR_GTF VI 1.0 and2                 
                JR_LTEQF VI 2.0 and3               
                JR_LTF 3.0 VI or1                                                      
              and3:                                
              and2:                                
                JR_LTEQF VI 4.0 else0_2            
                JR_NEQF VI 5.0 else0_2             
              or1:                                         
            ");
        }

        private void TestItFloat(string condition, string branchCode)
        {
            TestIt(@"
                i = 2
                If i = 0 Then
                    LCD.StopUpdate()
                ElseIf " + condition + @" Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                MOVEF_F 2.0 VI          
            " + branchCode + @"
                UI_BUTTON FLUSH
                JR endif0
              else0_2:
                UI_BUTTON WAIT_FOR_PRESS
              else0_3:                  
              endif0:   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileMultipleIfsAsIfElseIfs()
        {
            TestIt(@"
                i = 2
                If (i = 1) Or (i = 2) Then
                    Buttons.Flush()
                EndIf
                If (i = 3) Or (i = 4) Then
                    Buttons.Wait()
                EndIf
                If (i = 5) And (i = 6) Then
                    LCD.StopUpdate()
                EndIf
            ", @"
                MOVEF_F 2.0 VI              
                JR_EQF VI 1.0 or1           
                JR_NEQF VI 2.0 else0_1      
              or1:                                                      
                UI_BUTTON FLUSH          
              else0_1:                   
              endif0:                    
                JR_EQF VI 3.0 or3
                JR_NEQF VI 4.0 else2_1   
              or3:                       
                UI_BUTTON WAIT_FOR_PRESS 
              else2_1:                   
              endif2:                    
                JR_NEQF VI 5.0 else4_1   
                JR_NEQF VI 6.0 else4_1   
                MOVE32_32 1 STOPLCDUPDATE
              else4_1:                   
              endif4:                    
            ", ExtractMainProgramCode);
        }

    }
}

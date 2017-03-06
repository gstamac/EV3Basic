using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class IfTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsConstantStringTrue()         // NEW!!!!!  Optimized string branch
        {
            TestIt(@"
                If ""true"" Then
                    Buttons.Wait()
                Else
                    Buttons.Flush()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsConstantStringFalse()        // NEW!!!!!    Optimized string branch
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
        public void ShouldCompileIf_WhenConditionIsConstantString()     // NEW!!!!!   Optimized string branch
        {
            TestIt(@"
                If ""x"" Then
                    Buttons.Flush()
                Else
                    Buttons.Wait()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsAlwaysTrueComparingFloats()      // NEW!!!!!  Optimized comparison branch
        {
            TestIt(@"
                If 1 = 1 Then
                    Buttons.Wait()
                Else
                    Buttons.Flush()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsAlwaysTrue_ComparingStrings()        // NEW!!!!!   Optimized comparison branch
        {
            TestIt(@"
                If ""X"" = ""X"" Then
                    Buttons.Wait()
                Else
                    Buttons.Flush()
                EndIf
            ", @"
                UI_BUTTON WAIT_FOR_PRESS
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareEq()
        {
            TestItFloat("i = 1", @"
                JR_NEQF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareNotEq()
        {
            TestItFloat("i <> 1", @"
                JR_EQF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareGt()
        {
            TestItFloat("i > 1", @"
                JR_LTEQF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareGtEq()
        {
            TestItFloat("i >= 1", @"
                JR_LTF VI 1.0 else0_1");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLt()
        {
            TestItFloat("i < 1", @"
                JR_GTEQF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLtEq()
        {
            TestItFloat("i <= 1", @"
                JR_GTF VI 1.0 else0_1
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalOr()  // NEW!!!!! fixed bug JR_LTEF -> JR_LTEQF
        {
            TestItFloat("(i <= 1) Or (i > 2)", @"
                JR_LTEQF VI 1.0 or1      
                JR_LTEQF VI 2.0 else0_1 
              or1:                      
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalOrOr()
        {
            TestItFloat("(i = 1) Or (i = 2) Or (i = 3)", @"
                JR_EQF VI 1.0 or1       
                JR_EQF VI 2.0 or1       
                JR_NEQF VI 3.0 else0_1  
              or1:                      
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalOrOrOrOr()
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
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalAnd()
        {
            TestItFloat("(i <= 1) And (i > 2)", @"
                JR_GTF VI 1.0 else0_1   
                JR_LTEQF VI 2.0 else0_1 
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalAndAnd()
        {
            TestItFloat("(i <= 1) And (i > 2) And (i <> 3)", @"
                JR_GTF VI 1.0 else0_1   
                JR_LTEQF VI 2.0 else0_1 
                JR_EQF VI 3.0 else0_1   
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalAndAndAndAnd()
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
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalAndOrPrecedence()
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
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalOrAndPrecedence()
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
        public void ShouldCompileIf_WhenConditionIsFloatCompareLogicalComplicated()
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
                    Buttons.Wait()
                EndIf
            ", @"
                MOVEF_F 2.0 VI          
            " + branchCode + @"
                UI_BUTTON WAIT_FOR_PRESS
              else0_1:                  
              endif0:   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsString()     // NEW!!!!! Optimized string branch
        {
            TestItString(@"i", @"
                CALL IS_TRUE VI F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //STRINGS DUPLICATE VI S0
            //AND8888_32 S0 -538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareEq()        // NEW!!!!! Optimized string comparison branch
        {
            TestItString(@"i = ""1""", @"
                CALL EQ_STRING8 VI '1' F0              
                JR_EQ8 F0 0 else0_1                                              
            ");
            // PREVIOUS
            //CALL EQ_STRING VI '1' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareNotEq()     // NEW!!!!! Optimized string comparison branch
        {
            TestItString(@"i <> ""1""", @"
                CALL EQ_STRING8 VI '1' F0              
                JR_NEQ8 F0 0 else0_1                   
            ");
            // PREVIOUS
            //CALL NE_STRING VI '1' S0 
            //AND8888_32 S0 - 538976289 S0 
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalOr()
        {
            TestItString(@"(i = ""1"") Or (i <> ""2"")", @"
                CALL EQ_STRING8 VI '1' F0          
                JR_NEQ8 F0 0 or1                  
                CALL EQ_STRING8 VI '2' F0          
                JR_NEQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalOrOr()
        {
            TestItString(@"(i = ""1"") Or (i = ""2"") Or (i = ""3"")", @"
                CALL EQ_STRING8 VI '1' F0        
                JR_NEQ8 F0 0 or1                
                CALL EQ_STRING8 VI '2' F0        
                JR_NEQ8 F0 0 or1                                                                       
                CALL EQ_STRING8 VI '3' F0    
                JR_EQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalOrOrOrOr()
        {
            TestItString(@"(i = ""1"") Or (i = ""2"") Or (i = ""3"") Or (i = ""4"") Or (i = ""5"")", @"
                CALL EQ_STRING8 VI '1' F0       
                JR_NEQ8 F0 0 or1               
                CALL EQ_STRING8 VI '2' F0       
                JR_NEQ8 F0 0 or1               
                CALL EQ_STRING8 VI '3' F0       
                JR_NEQ8 F0 0 or1            
                CALL EQ_STRING8 VI '4' F0    
                JR_NEQ8 F0 0 or1            
                CALL EQ_STRING8 VI '5' F0    
                JR_EQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalAnd()
        {
            TestItString(@"(i = ""1"") And (i <> ""2"")", @"
                CALL EQ_STRING8 VI '1' F0             
                JR_EQ8 F0 0 else0_1                  
                CALL EQ_STRING8 VI '2' F0             
                JR_NEQ8 F0 0 else0_1         
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalAndAnd()
        {
            TestItString(@"(i = ""1"") And (i = ""2"") And (i = ""3"")", @"
                CALL EQ_STRING8 VI '1' F0           
                JR_EQ8 F0 0 else0_1                
                CALL EQ_STRING8 VI '2' F0           
                JR_EQ8 F0 0 else0_1         
                CALL EQ_STRING8 VI '3' F0    
                JR_EQ8 F0 0 else0_1         
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalAndAndAndAnd()
        {
            TestItString(@"(i = ""1"") And (i = ""2"") And (i = ""3"") And (i = ""4"") And (i = ""5"")", @"
                CALL EQ_STRING8 VI '1' F0           
                JR_EQ8 F0 0 else0_1                
                CALL EQ_STRING8 VI '2' F0           
                JR_EQ8 F0 0 else0_1                
                CALL EQ_STRING8 VI '3' F0                                                    
                JR_EQ8 F0 0 else0_1         
                CALL EQ_STRING8 VI '4' F0    
                JR_EQ8 F0 0 else0_1         
                CALL EQ_STRING8 VI '5' F0    
                JR_EQ8 F0 0 else0_1         
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalAndOrPrecedence()
        {
            //  == ((i <= 1) And (i > 2)) Or (3 < i)
            TestItString(@"(i = ""1"") And (i = ""2"") Or (""3"" = i)", @"
                CALL EQ_STRING8 VI '1' F0            
                JR_EQ8 F0 0 and2                    
                CALL EQ_STRING8 VI '2' F0            
                JR_NEQ8 F0 0 or1                    
              and2:                         
                CALL EQ_STRING8 '3' VI F0    
                JR_EQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalOrAndPrecedence()
        {
            //  == (i <= 1) Or ((i > 2) And (3 < i))
            TestItString(@"(i = ""1"") Or (i = ""2"") And (""3"" = i)", @"
                CALL EQ_STRING8 VI '1' F0             
                JR_NEQ8 F0 0 or1                     
                CALL EQ_STRING8 VI '2' F0             
                JR_EQ8 F0 0 else0_1         
                CALL EQ_STRING8 '3' VI F0    
                JR_EQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsStringCompareLogicalComplicated()
        {
            TestItString(@"(i = ""1"") And ((i = ""2"") And (""3"" <> i)) Or ((i = ""4"") And (i <> ""5""))", @"
                CALL EQ_STRING8 VI '1' F0          
                JR_EQ8 F0 0 and2                  
                CALL EQ_STRING8 VI '2' F0          
                JR_EQ8 F0 0 and3                  
                CALL EQ_STRING8 '3' VI F0          
                JR_EQ8 F0 0 or1            
              and3:                                                                       
              and2:                         
                CALL EQ_STRING8 VI '4' F0    
                JR_EQ8 F0 0 else0_1         
                CALL EQ_STRING8 VI '5' F0    
                JR_NEQ8 F0 0 else0_1         
              or1:                          
            ");
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsExternalFunction()       // NEW!!!!! Optimized string branch
        {
            TestItString(@"Text.IsSubText(i, ""U"")", @"
                CALL TEXT.ISSUBTEXT VI 'U' S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL TEXT.ISSUBTEXT VI 'U' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 F0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingExternalFunction()      // NEW!!!!! Optimized string comparison branch
        {
            TestItString(@"Text.IsSubText(i, ""U"") = ""X""", @"
                CALL TEXT.ISSUBTEXT VI 'U' S0
                CALL EQ_STRING8 S0 'X' F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL TEXT.ISSUBTEXT VI 'U' S1
            //CALL EQ_STRING S1 'X' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsExternalProperty()       // NEW!!!!! Optimized string branch
        {
            TestItString(@"Buttons.Current", @"
                CALL BUTTONS.CURRENT S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL BUTTONS.CURRENT S0
            //AND8888_32 S0 -538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingExternalProperty()      // NEW!!!!! Optimized string comparison branch
        {
            TestItString(@"Buttons.Current = ""X""", @"
                CALL BUTTONS.CURRENT S0
                CALL EQ_STRING8 S0 'X' F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL BUTTONS.CURRENT S1
            //CALL EQ_STRING S1 'X' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsInlineExternalFunction()     // NEW!!!!! Optimized string branch
        {
            TestItString(@"Mailbox.Receive(8)", @"
                DATA8 no1
                MOVEF_8 8.0 no1
                MAILBOX_READY no1
                MAILBOX_READ no1 252 1 S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //DATA8 no1
            //MOVEF_8 8.0 no1
            //MAILBOX_READY no1
            //MAILBOX_READ no1 252 1 S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingInlineExternalFunction()        // NEW!!!!! Optimized string comparison branch
        {
            TestItString(@"Mailbox.Receive(8) = ""X""", @"
                DATA8 no1
                MOVEF_8 8.0 no1
                MAILBOX_READY no1
                MAILBOX_READ no1 252 1 S0
                CALL EQ_STRING8 S0 'X' F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //DATA8 no1
            //MOVEF_8 8.0 no1
            //MAILBOX_READY no1
            //MAILBOX_READ no1 252 1 S1
            //CALL EQ_STRING S1 'X' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingVariableToTrue()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"i = ""true""", @"
                CALL IS_TRUE VI F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'true' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingTrueToVariable()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"""true"" = i", @"
                CALL IS_TRUE VI F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'true' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingVariableToFalse()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"i = ""false""", @"
                CALL IS_TRUE VI F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'false' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingFalseToVariable()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"""false"" = i", @"
                CALL IS_TRUE VI F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'false' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingVariableToTrue_NotEqual()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"i <> ""true""", @"
                CALL IS_TRUE VI F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'true' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_NEQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingTrueToVariable_NotEqual()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"""true"" <> i", @"
                CALL IS_TRUE VI F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'true' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_NEQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingVariableToFalse_NotEqual()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"i <> ""false""", @"
                CALL IS_TRUE VI F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'false' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_NEQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingFalseToVariable_NotEqual()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"""false"" <> i", @"
                CALL IS_TRUE VI F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL EQ_STRING VI 'false' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_NEQ8 S0 0 else0_1
        }

        [TestMethod] 
        public void ShouldCompileIf_WhenConditionIsComparingExternalFunctionToTrue()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"Text.IsSubText(i, ""U"") = ""true""", @"
                CALL TEXT.ISSUBTEXT VI 'U' S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL TEXT.ISSUBTEXT VI 'U' S1
            //CALL EQ_STRING S1 'true' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingExternalFunctionToFalse()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"Text.IsSubText(i, ""U"") = ""false""", @"
                CALL TEXT.ISSUBTEXT VI 'U' S0
                CALL IS_TRUE S0 F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL TEXT.ISSUBTEXT VI 'U' S1
            //CALL EQ_STRING S1 'false' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingExternalPropertyToTrue()      // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"Buttons.Current = ""True""", @"
                CALL BUTTONS.CURRENT S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL BUTTONS.CURRENT S1
            //CALL EQ_STRING S1 'True' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingExternalPropertyToFalse()      // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"Buttons.Current = ""False""", @"
                CALL BUTTONS.CURRENT S0
                CALL IS_TRUE S0 F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //CALL BUTTONS.CURRENT S1
            //CALL EQ_STRING S1 'False' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingInlineExternalFunctionToTrue()        // NEW!!!!! Optimized string branch when comparing to true
        {
            TestItString(@"Mailbox.Receive(8) = ""TRUE""", @"
                DATA8 no1
                MOVEF_8 8.0 no1
                MAILBOX_READY no1
                MAILBOX_READ no1 252 1 S0
                CALL IS_TRUE S0 F0
                JR_EQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //DATA8 no1
            //MOVEF_8 8.0 no1
            //MAILBOX_READY no1
            //MAILBOX_READ no1 252 1 S1
            //CALL EQ_STRING S1 'TRUE' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        [TestMethod]
        public void ShouldCompileIf_WhenConditionIsComparingInlineExternalFunctionToFalse()        // NEW!!!!! Optimized string branch when comparing to false
        {
            TestItString(@"Mailbox.Receive(8) = ""FALSE""", @"
                DATA8 no1
                MOVEF_8 8.0 no1
                MAILBOX_READY no1
                MAILBOX_READ no1 252 1 S0
                CALL IS_TRUE S0 F0
                JR_NEQ8 F0 0 else0_1
            ");
            // PREVIOUS
            //DATA8 no1
            //MOVEF_8 8.0 no1
            //MAILBOX_READY no1
            //MAILBOX_READ no1 252 1 S1
            //CALL EQ_STRING S1 'FALSE' S0
            //AND8888_32 S0 - 538976289 S0
            //STRINGS COMPARE S0 'TRUE' S0
            //JR_EQ8 S0 0 else0_1
        }

        private void TestItString(string condition, string branchCode)
        {
            TestIt(@"
                i = ""X""
                If " + condition + @" Then
                    Buttons.Wait()
                EndIf
            ", @"
                STRINGS DUPLICATE 'X' VI          
            " + branchCode + @"
                UI_BUTTON WAIT_FOR_PRESS
              else0_1:                  
              endif0:   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileMultipleIfs()      // NEW!!!!! optimized label numbering
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

        [TestMethod]
        public void ShouldFail_WhenComparingDifferentTypes()
        {
            TestCompileFailure(@"
                If (1 = ""X"") Then
                    Buttons.Wait()
                EndIf
            ", "Boolean operations on unrelated types are not permited", 2, 20);
        }

        [TestMethod]
        public void ShouldFail_WhenComparingArrays()
        {
            TestCompileFailure(@"
                i[3] = 10
                If (i = i) Then
                    Buttons.Wait()
                EndIf
            ", "Boolean operations on arrays are not permited", 3, 20);
        }

        [TestMethod]
        public void ShouldFail_WhenComparingStringsWithGt()
        {
            TestCompileFailure(@"
                If (""x"" > ""y"") Then
                    Buttons.Wait()
                EndIf
            ", "Only (non)equality comparison is permited on strings", 2, 20);
        }

        [TestMethod]
        public void ShouldFail_WhenComparingStringsWithGtEq()
        {
            TestCompileFailure(@"
                If (""x"" >= ""y"") Then
                    Buttons.Wait()
                EndIf
            ", "Only (non)equality comparison is permited on strings", 2, 20);
        }

        [TestMethod]
        public void ShouldFail_WhenComparingStringsWithLt()
        {
            TestCompileFailure(@"
                If (""x"" < ""y"") Then
                    Buttons.Wait()
                EndIf
            ", "Only (non)equality comparison is permited on strings", 2, 20);
        }

        [TestMethod]
        public void ShouldFail_WhenComparingStringsWithLtEq()
        {
            TestCompileFailure(@"
                If (""x"" <= ""y"") Then
                    Buttons.Wait()
                EndIf
            ", "Only (non)equality comparison is permited on strings", 2, 20);
        }
    }
}

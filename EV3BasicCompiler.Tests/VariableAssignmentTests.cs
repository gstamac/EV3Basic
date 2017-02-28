using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class VariableAssignmentTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldAssignInt()
        {
            TestIt(@"
                x = 10
            ", @"
                MOVEF_F 10.0 VX
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignInt_WhenValueIsNegative()
        {
            TestIt(@"
                x = -10
            ", @"
                MOVEF_F -10.0 VX
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat()
        {
            TestIt(@"
                x = 10.23
            ", @"
                MOVEF_F 10.23 VX
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenValueIsNegative()
        {
            TestIt(@"
                x = -10.23
            ", @"
                MOVEF_F -10.23 VX
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString()
        {
            TestIt(@"
                x = ""XX""
            ", @"
                STRINGS DUPLICATE 'XX' VX
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignIntArray()
        {
            TestIt(@"
                i[6] = 2
            ", @"
                CALL ARRAYSTORE_FLOAT 6.0 2.0 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignIntArray_NoBoundsCheck()
        {
            TestIt(@"
                'PRAGMA NOBOUNDSCHECK
                i[6] = 2
            ", @"
                ARRAY_WRITE VI 6 2.0
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloatArray()
        {
            TestIt(@"
                i[6] = 2.3
            ", @"
                CALL ARRAYSTORE_FLOAT 6.0 2.3 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_NoBoundsCheck()
        {
            TestIt(@"
                'PRAGMA NOBOUNDSCHECK
                i[6] = 2.3
            ", @"
                ARRAY_WRITE VI 6 2.3
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_WithFormulaIndexer()
        {
            TestIt(@"
                i[2 + 4] = 2
            ", @"
                CALL ARRAYSTORE_FLOAT 6.0 2.0 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_WithReferenceFormulaIndexer()
        {
            TestIt(@"
                j = 3
                i[j + 4] = 2
            ", @"
                MOVEF_F 3.0 VJ                   
                ADDF VJ 4.0 F0                            
                CALL ARRAYSTORE_FLOAT F0 2.0 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignStringArray()
        {
            TestIt(@"
                i[7] = ""X""
            ", @"
                CALL ARRAYSTORE_STRING 7.0 'X' VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignStringArray_WhenUsingCondensedFormat() // NEW!!!!!
        {
            TestIt(@"
                i = ""1=1;2=2;3=3""
            ", @"
                CALL ARRAYSTORE_STRING 1.0 '1' VI   
                CALL ARRAYSTORE_STRING 2.0 '2' VI   
                CALL ARRAYSTORE_STRING 3.0 '3' VI          
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithFormula()
        {
            TestIt(@"
                i = 10.3 + 10
            ", @"
                MOVEF_F 20.3 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithFormulaNegative()
        {
            TestIt(@"
                i = -(10.3 + 10)
            ", @"
                MOVEF_F -20.3 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithFormulaMultiple()
        {
            TestIt(@"
                i = (10 - 8) * 3 + 6
            ", @"
                MOVEF_F 12.0 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithFormula()
        {
            TestIt(@"
                i = ""X"" + ""Y""
            ", @"
                STRINGS DUPLICATE 'XY' VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignStringWithConvert_WhenAssignedWithFloat()  // NEW!!!!!
        {
            TestIt(@"
                i = ""X""
                i = 10
            ", @"
                STRINGS DUPLICATE 'X' VI
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithMixedFormula()
        {
            TestIt(@"
                i = ""X"" + 10
                j = 10 + ""X""
            ", @"
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 S0
                CALL TEXT.APPEND 'X' S0 VI
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 S0
                CALL TEXT.APPEND S0 'X' VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithMixedNumberFormula()
        {
            TestIt(@"
                i = ""1"" + 10
                j = 10 + ""1""
            ", @"
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 S0  
                CALL TEXT.APPEND '1' S0 VI               
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 S0  
                CALL TEXT.APPEND S0 '1' VJ                                          
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReference()
        {
            TestIt(@"
                i = 10.3
                j = i
            ", @"
                MOVEF_F 10.3 VI
                MOVEF_F VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithNegativeReference()
        {
            TestIt(@"
                i = 10.3
                j = -i
            ", @"
                MOVEF_F 10.3 VI
                MATH NEGATE VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSimpleAddition()
        {
            TestIt(@"
                i = 10.3
                j = i + i
            ", @"
                MOVEF_F 10.3 VI
                ADDF VI VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSimpleSubtraction()
        {
            TestIt(@"
                i = 10.3
                j = i - i
            ", @"
                MOVEF_F 10.3 VI
                SUBF VI VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSimpleMultiplication()
        {
            TestIt(@"
                i = 10.3
                j = i * i
            ", @"
                MOVEF_F 10.3 VI
                MULF VI VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaNegativeMultiplication()
        {
            TestIt(@"
                i = 10.3
                j = i * -i
            ", @"
                MOVEF_F 10.3 VI
                MATH NEGATE VI F0
                MULF VI F0 VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSimpleDivisionWithoutCheck()
        {
            TestIt(@"
                'PRAGMA NODIVISIONCHECK

                i = 10.3
                j = i / i
            ", @"
                MOVEF_F 10.3 VI
                DIVF VI VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSimpleDivision()
        {
            TestIt(@"
                i = 10.3
                j = i / i
            ", @"
                MOVEF_F 10.3 VI
                DATAF tmpf0
                DATA8 flag0
                DIVF VI VI tmpf0
                CP_EQF 0.0 VI flag0
                SELECTF flag0 0.0 tmpf0 VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaMultipleDivisions()
        {
            TestIt(@"
                i = 10.3
                j = i / i / i
            ", @"
                MOVEF_F 10.3 VI
                DATAF tmpf0
                DATA8 flag0
                DIVF VI VI tmpf0
                CP_EQF 0.0 VI flag0
                SELECTF flag0 0.0 tmpf0 F0

                DATAF tmpf1
                DATA8 flag1
                DIVF F0 VI tmpf1
                CP_EQF 0.0 VI flag1
                SELECTF flag1 0.0 tmpf1 VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaSelfReference()
        {
            TestIt(@"
                i = 10.3
                j = 3
                j = j + i
            ", @"
                MOVEF_F 10.3 VI
                MOVEF_F 3.0 VJ
                ADDF VJ VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaWithLiteral()
        {
            TestIt(@"
                i = 10.3
                j = i + 6
            ", @"
                MOVEF_F 10.3 VI
                ADDF VI 6.0 VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaMultiple()
        {
            TestIt(@"
                i = 10.3
                j = 3
                k = 5.3
                l = (i - j) * k / i
            ", @"
                MOVEF_F 10.3 VI             
                MOVEF_F 3.0 VJ              
                MOVEF_F 5.3 VK              
                SUBF VI VJ F1               
                MULF F1 VK F0               
                DATAF tmpf2                 
                DATA8 flag2                 
                DIVF F0 VI tmpf2            
                CP_EQF 0.0 VI flag2                   
                SELECTF flag2 0.0 tmpf2 VL
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaMultipleNoDivisionCheck()
        {
            TestIt(@"
                'PRAGMA NODIVISIONCHECK

                i = 10.3
                j = 3
                k = 5.3
                l = (i - j) * k / i
            ", @"
                MOVEF_F 10.3 VI 
                MOVEF_F 3.0 VJ  
                MOVEF_F 5.3 VK  
                SUBF VI VJ F0   
                MULF F0 VK F1   
                DIVF F1 VI VL                                 
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaMultipleWithNegative()
        {
            TestIt(@"
                i = 10.3
                j = 3
                k = 5.3
                l = (i - j) * -k / i
            ", @"
                MOVEF_F 10.3 VI
                MOVEF_F 3.0 VJ
                MOVEF_F 5.3 VK
                SUBF VI VJ F1
                MATH NEGATE VK F2
                MULF F1 F2 F0
                DATAF tmpf3
                DATA8 flag3
                DIVF F0 VI tmpf3
                CP_EQF 0.0 VI flag3
                SELECTF flag3 0.0 tmpf3 VL
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithReferenceFormulaMultipleWithNegativeNoDivisionCheck()
        {
            TestIt(@"
                'PRAGMA NODIVISIONCHECK

                i = 10.3
                j = 3
                k = 5.3
                l = (i - j) * -k / i
            ", @"
                MOVEF_F 10.3 VI
                MOVEF_F 3.0 VJ
                MOVEF_F 5.3 VK
                SUBF VI VJ F0
                MATH NEGATE VK F1
                MULF F0 F1 F2
                DIVF F2 VI VL
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithIndexedReference()
        {
            TestIt(@"
                i[2] = 10.3
                j = i[2]
            ", @"
                CALL ARRAYSTORE_FLOAT 2.0 10.3 VI
                CALL ARRAYGET_FLOAT 2.0 VJ VI
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WhenAssignedWithIndexedReferenceFormula()
        {
            TestIt(@"
                i[2] = 10.3
                j = i[1] + i[2]
            ", @"
                CALL ARRAYSTORE_FLOAT 2.0 10.3 VI 
                CALL ARRAYGET_FLOAT 1.0 F0 VI     
                CALL ARRAYGET_FLOAT 2.0 F1 VI                   
                ADDF F0 F1 VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloat_WithReferenceFormulaIndexer()
        {
            TestIt(@"
                k = 3
                i[2] = 10.3
                j = i[k + 1]
            ", @"
                MOVEF_F 3.0 VK                     
                CALL ARRAYSTORE_FLOAT 2.0 10.3 VI  
                ADDF VK 1.0 F0                     
                CALL ARRAYGET_FLOAT F0 VJ VI                                                          
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithReference()
        {
            TestIt(@"
                i = ""X""
                j = i
            ", @"
                STRINGS DUPLICATE 'X' VI
                STRINGS DUPLICATE VI VJ
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithIndexedReference()
        {
            TestIt(@"
                i[2] = ""X""
                j = i[1]
            ", @"
                CALL ARRAYSTORE_STRING 2.0 'X' VI
                CALL ARRAYGET_STRING 1.0 VJ VI         
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithReferenceFormula()
        {
            TestIt(@"
                i = ""X""
                j = i + i
            ", @"
                STRINGS DUPLICATE 'X' VI     
                CALL TEXT.APPEND VI VI VJ 
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithIndexedReferenceFormula()
        {
            TestIt(@"
                i[23] = ""X""
                j = i[1] + i[2]
            ", @"
                CALL ARRAYSTORE_STRING 23.0 'X' VI  
                CALL ARRAYGET_STRING 1.0 S0 VI      
                CALL ARRAYGET_STRING 2.0 S1 VI                          
                CALL TEXT.APPEND S0 S1 VJ  
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignString_WhenAssignedWithMixedReferenceFormula()
        {
            TestIt(@"
                i = ""X""
                j = 10
                k = i + j
                l = j + i
            ", @"
                STRINGS DUPLICATE 'X' VI               
                MOVEF_F 10.0 VJ                        
                STRINGS VALUE_FORMATTED VJ '%g' 99 S0  
                CALL TEXT.APPEND VI S0 VK                       
                STRINGS VALUE_FORMATTED VJ '%g' 99 S0    
                CALL TEXT.APPEND S0 VI VL
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldFail_WhenAssigningStringWithNegative()
        {
            TestCompileFailure(@"
                i = -""X""
            ", "Need number after minus", 2, 21);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_WhenAssignedWithReference()
        {
            TestIt(@"
                i[3] = 10.3
                j = i
            ", @"
                CALL ARRAYSTORE_FLOAT 3.0 10.3 VI  
                ARRAY COPY VI VJ            
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_WhenAssignedWithReferenceToSelf()
        {
            TestIt(@"
                i[3] = 10.3
                i[2] = i[3]
            ", @"
                CALL ARRAYSTORE_FLOAT 3.0 10.3 VI 
                CALL ARRAYGET_FLOAT 3.0 F0 VI                                       
                CALL ARRAYSTORE_FLOAT 2.0 F0 VI          
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignStringArray_WhenAssignedWithReference()
        {
            TestIt(@"
                i[4] = ""X""
                j = i
            ", @"
                CALL ARRAYSTORE_STRING 4.0 'X' VI
                ARRAY COPY VI VJ 
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldFail_WhenAssigningArrayWithoutIndex()
        {
            TestCompileFailure(@"
                i[2] = 10
                i = 10
            ", "Cannot assign value to array variable 'i' without index", 3, 17);

        }

        [TestMethod]
        public void ShouldFail_WhenAssigningNonArrayVariableWithIndex()
        {
            TestCompileFailure(@"
                i = 10
                i[2] = 10
            ", "Cannot use index on non-array variable 'i'", 3, 17);
        }

        [TestMethod]
        public void ShouldFail_WhenAssigningArrayVariableToNonArrayVariable()
        {
            TestCompileFailure(@"
                i[2] = 10
                j = 10
                j = i
            ", "Cannot assign array value to non-array variable 'j'", 4, 17);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignStringArrayWithConvert_WhenAssignedWithFloat()  // NEW!!!!!
        {
            TestIt(@"
                i[2] = ""X""
                i[3] = 10
            ", @"
                CALL ARRAYSTORE_STRING 2.0 'X' VI  
                STRINGS VALUE_FORMATTED 10.0 '%g' 99 S0
                CALL ARRAYSTORE_STRING 3.0 S0 VI    
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignStringArrayWithConvert_WhenAssignedWithFloatArray()  // NEW!!!!!
        {
            TestIt(@"
                i[2] = ""X""
                j[1] = 10
                i[3] = j[1]
            ", @"
                CALL ARRAYSTORE_STRING 2.0 'X' VI  
                CALL ARRAYSTORE_FLOAT 1.0 10.0 VJ  
                CALL ARRAYGET_FLOAT 1.0 VI F0                         
                STRINGS VALUE_FORMATTED F0 '%g' 99 S0
                CALL ARRAYSTORE_STRING 3.0 S0 VI   
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldFail_WhenSameIdentifierIsUsedForDifferentTypes()
        {
            TestCompileFailure(@"
                i = 10
                i = ""X""
            ", "Cannot assign String value to Float variable 'i'", 3, 17);
        }

        [TestMethod]
        public void ShouldAssignFloatArray_WhenReferencingExternalFunction()
        {
            TestIt(@"
                raw = Sensor.ReadRaw(1, 8)
            ", @"
                CALL SENSOR.READRAW 1.0 8.0 VRAW
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAssignFloatWithConvert_WhenReferencingExternalFunction() // NEW!!!!!
        {
            TestIt(@"
                raw = ""X""
                raw = Sensor.ReadRawValue(1, 8)
            ", @"
                STRINGS DUPLICATE 'X' VRAW
                CALL SENSOR.READRAW 1.0 8.0 F0
                STRINGS VALUE_FORMATTED F0 '%g' 99 VRAW 
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldAssignInt_WhenReferencingExternalProperty()
        {
            TestIt(@"
                i = Buttons.Current
            ", @"
                CALL BUTTONS.CURRENT VI 
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        [Ignore]
        public void ShouldFail_WhenAssigningUnknownProperty()
        {
            TestCompileFailure(@"
                Unknown.Property = 10
            ", "Unknown property to set", 0, 0);
        }
    }
}

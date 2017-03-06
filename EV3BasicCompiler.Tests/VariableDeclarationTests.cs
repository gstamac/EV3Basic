using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FluentAssertions;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class VariableDeclarationTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldDeclareInt()
        {
            TestDeclaration(@"
                i = 10
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenValueIsNegative()
        {
            TestDeclaration(@"
                i = -10
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat()
        {
            TestDeclaration(@"
                i = 10.3
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenValueIsNegative()
        {
            TestDeclaration(@"
                i = -10.3
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareString()
        {
            TestDeclaration(@"
                i = ""X""
            ", @"
                DATAS VI 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareIntArray()
        {
            TestDeclaration(@"
                i[6] = 2
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray()
        {
            TestDeclaration(@"
                i[6] = 2.3
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray_WithFormulaIndexer()
        {
            TestDeclaration(@"
                i[2 + 4] = 2
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray_WithReferenceFormulaIndexer()
        {
            TestDeclaration(@"
                j = 3
                i[j + 4] = 2
            ", @"
                DATAF VJ
                ARRAY16 VI 2
            ");
        }

        [TestMethod]
        public void ShouldDeclareStringArray()
        {
            TestDeclaration(@"
                i[7] = ""X""
            ", @"
                ARRAY16 VI 7
            ");
        }

        [TestMethod]
        public void ShouldDeclareStringArray_WhenUsingCondensedFormat()
        {
            TestDeclaration(@"
                i = ""1=1;2=2;3=3""
            ", @"
                ARRAY16 VI 3
            ");
            TestDeclaration(@"
                i = ""1=1;2=2;3=3;""
            ", @"
                ARRAY16 VI 3
            ");
        }

        [TestMethod]
        public void ShouldDeclareArrayWithMaxIndex()
        {
            TestDeclaration(@"
                i[2] = 2
                i[6] = 2
                i[3] = 2
            ", @"
                ARRAY16 VI 6
            ");
        }

        [TestMethod]
        public void ShouldDeclareArrayWithMaxIndex_WhenReferenced()
        {
            TestDeclaration(@"
                i[2] = 2
                j = i[12]
            ", @"
                ARRAY16 VI 12
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareArrayWithMaxIndex_WhenReferencedInFormula()
        {
            TestDeclaration(@"
                i[2] = 2
                j = i[3] + i[12]
            ", @"
                ARRAY16 VI 12
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithFormula()
        {
            TestDeclaration(@"
                i = 10.3 + 10
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithFormula()
        {
            TestDeclaration(@"
                i = ""X"" + ""Y""
            ", @"
                DATAS VI 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithMixedFormula()
        {
            TestDeclaration(@"
                i = ""X"" + 10
                j = 10 + ""X""
            ", @"
                DATAS VI 252
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithReference()
        {
            TestDeclaration(@"
                i = 10.3
                j = i
            ", @"
                DATAF VI
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithNegativeReference()
        {
            TestDeclaration(@"
                i = 10.3
                j = -i
            ", @"
                DATAF VI
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithIndexedReference()
        {
            TestDeclaration(@"
                i[2] = 10.3
                j = i[2]
            ", @"
                ARRAY16 VI 2
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithReferenceFormula()
        {
            TestDeclaration(@"
                i = 10.3
                j = i + i
            ", @"
                DATAF VI
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenDeclaredWithIndexedReferenceFormula()
        {
            TestDeclaration(@"
                i[2] = 10.3
                j = i[1] + i[2]
            ", @"
                ARRAY16 VI 2
                DATAF VJ
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithReference()
        {
            TestDeclaration(@"
                i = ""X""
                j = i
            ", @"
                DATAS VI 252
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithIndexedReference()
        {
            TestDeclaration(@"
                i[2] = ""X""
                j = i[1]
            ", @"
                ARRAY16 VI 2
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithReferenceFormula()
        {
            TestDeclaration(@"
                i = ""X""
                j = i + i
            ", @"
                DATAS VI 252
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithIndexedReferenceFormula()
        {
            TestDeclaration(@"
                i[23] = ""X""
                j = i[1] + i[2]
            ", @"
                ARRAY16 VI 23
                DATAS VJ 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareString_WhenDeclaredWithMixedReferenceFormula()
        {
            TestDeclaration(@"
                i = ""X""
                j = 10
                k = i + j
                l = j + i
            ", @"
                DATAS VI 252
                DATAF VJ
                DATAS VK 252
                DATAS VL 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray_WhenDeclaredWithReference()
        {
            TestDeclaration(@"
                i[3] = 10.3
                j = i
            ", @"
                ARRAY16 VI 3
                ARRAY16 VJ 3
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatArray_WhenDeclaredWithReferenceAndReindexed()
        {
            TestDeclaration(@"
                i[3] = 10.3
                j = i
                i[5] = 3
            ", @"
                ARRAY16 VI 5
                ARRAY16 VJ 3
            ");
        }

        [TestMethod]
        public void ShouldDeclareStringArray_WhenDeclaredWithReference()
        {
            TestDeclaration(@"
                i[4] = ""X""
                j = i
            ", @"
                ARRAY16 VI 4
                ARRAY16 VJ 4
            ");
        }

        [TestMethod]
        public void ShouldFail_WhenAssigningStringArrayWithFormula()
        {
            TestCompileFailure(@"
                i[4] = ""X""
                j = i + i
            ", "Operations on arrays are not permited", 3, 21);
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoop()
        {
            TestDeclaration(@"
                For i = 10 To 24
                EndFor
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoopWithFormulaDeclaration()
        {
            TestDeclaration(@"
                j = 0
                For i = j + 1 To 24
                EndFor
            ", @"
                DATAF VJ
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoopWithFormulaDeclarationForTo()
        {
            TestDeclaration(@"
                j = 0
                For i = 10 To j + 10
                EndFor
            ", @"
                DATAF VJ
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenUsedInForLoopWithStep()
        {
            TestDeclaration(@"
                For i = 10 To 24 Step 2
                EndFor
            ", @"
                DATAF VI
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenReferencingExternalFunction()
        {
            TestDeclaration(@"
                raw = Sensor.ReadRaw(1, 8)
            ", @"
                ARRAY16 VRAW 2
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenReferencingExternalInlineFunction()
        {
            TestDeclaration(@"
                i = Mailbox.Receive(8)
            ", @"
                DATAS VI 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareInt_WhenReferencingExternalProperty()
        {
            TestDeclaration(@"
                i = Buttons.Current
            ", @"
                DATAS VI 252
            ");
        }

        [TestMethod]
        public void ShouldDeclareFloatTempVariables()
        {
            TestIt(@"
                LCD.Text(1, 50, EV3.BatteryLevel, 2, """")
            ", @"
                DATAF F0
            ", ExtractTempDeclarationCode);

        }

        [TestMethod]
        public void ShouldDeclareStringTempVariables()
        {
            TestIt(@"
                LCD.Text(1, 50, 50, 2, 3)
            ", @"
                DATAS S0 252
            ", ExtractTempDeclarationCode);

        }

        [TestMethod]
        public void ShouldDeclareFloatAndStringTempVariables()
        {
            TestIt(@"
                LCD.Text(1, EV3.BatteryLevel, EV3.BatteryLevel, EV3.BatteryLevel, EV3.BatteryLevel)
                Text.IsSubText(1, 2)
            ", @"
                DATAF F0           
                DATAF F1                      
                DATAF F2    
                DATAF F3    
                DATAS S0 252
                DATAS S1 252
                DATAS S2 252
            ", ExtractTempDeclarationCode);

        }

        [TestMethod]
        public void ShouldDeclareTempVariables_WhenUsedForInlineFunction()
        {
            TestIt(@"
                F = Math.Remainder(Math.Floor(EV3.Time / 3000), 3)
            ", @"
                DATAF F0           
                DATAF F1                      
            ", ExtractTempDeclarationCode);
        }

        [TestMethod]
        public void ShouldDeclareTempVariables_WhenUsedInsideSub()
        {
            TestIt(@"
                Text.IsSubText(1, 2)

                Sub X
                    Text.IsSubText(1, 2)
                EndSub
            ", @"
                DATAS S0 252              
                DATAS S1 252              
                DATAS S2 252                                 
            ", ExtractTempDeclarationCode);
        }

        protected string ExtractTempDeclarationCode(string ev3Code)
        {
            ev3Code = ExtractFullMainProgramCode(ev3Code);

            Match match = Regex.Match(ev3Code, @"ARRAY8 STACKPOINTER 4([^}]*)MOVE8_8 0 STACKPOINTER", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            ev3Code = Regex.Replace(ev3Code, "[\n\r \t]*ARRAY32 RETURNSTACK2? 128", "");
            return ev3Code;
        }

    }
}

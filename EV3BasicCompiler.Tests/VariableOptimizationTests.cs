using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class VariableOptimizationTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldNotInitializeConstantFloat_WhenUsedAsConstant()
        {
            TestInitialization(@"
                i = 8
                Sensor.ReadRawValue(1, i)
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareConstantFloat_WhenUsedAsConstant()
        {
            TestDeclaration(@"
                i = 8
                Sensor.ReadRawValue(1, i)
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldReplaceFloatVariableWithConstantValue()
        {
            TestIt(@"
                i = 8
                Sensor.ReadRawValue(1, i)
            ", @"
                CALL SENSOR.READRAWVALUE 1.0 8.0 F0
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldReplaceFloatVariableWithConstantValue_WhenDefinedInConstantIf()
        {
            TestIt(@"
                If ""true"" Then
                    i = 8
                EndIf
                Sensor.ReadRawValue(1, i)
            ", @"
                CALL SENSOR.READRAWVALUE 1.0 8.0 F0
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldReplaceFloatVariableWithConstantValue_WhenDefinedInConstantElseIf()
        {
            TestIt(@"
                If ""false"" Then
                ElseIf ""true"" Then
                    i = 8
                EndIf
                Sensor.ReadRawValue(1, i)
            ", @"
                CALL SENSOR.READRAWVALUE 1.0 8.0 F0
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldReplaceFloatVariableWithConstantValue_WhenDefinedInConstantElse()
        {
            TestIt(@"
                If ""false"" Then
                Else
                    i = 8
                EndIf
                Sensor.ReadRawValue(1, i)
            ", @"
                CALL SENSOR.READRAWVALUE 1.0 8.0 F0
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldNotInitializeConstantString_WhenUsedAsConstant()
        {
            TestInitialization(@"
                i = ""X""
                Assert.Failed(i)
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldNotInitializeConstantString_WhenReassignedAndUsedAsConstant()
        {
            TestInitialization(@"
                i = ""X""
                i = 10
                Assert.Failed(i)
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareConstantString_WhenUsedAsConstant()
        {
            TestDeclaration(@"
                i = ""X""
                Assert.Failed(i)
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldReplaceStringVariableWithConstantValue()
        {
            TestIt(@"
                i = ""X""
                Assert.Failed(i)
            ", @"
                CALL ASSERT.FAILED 'X'
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldReplaceFloatVariableWithConstantValueConvertedToString()
        {
            TestIt(@"
                i = 10
                Assert.Failed(i)
            ", @"
                CALL ASSERT.FAILED '10.0'
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldReplaceStringVariableWithConstantValue_WhenDefinedInConstantIf()
        {
            TestIt(@"
                If ""true"" Then
                    i = ""X""
                EndIf
                Assert.Failed(i)
            ", @"
                CALL ASSERT.FAILED 'X'
            ", ExtractMainProgramCode, true);
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenUsedOnlyInForLoop()
        {
            TestDeclaration(@"
                For i = 10 To 24
                EndFor
            ", @"
                DATAF VI
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareFloat_WhenAssignedMultipleTimes()
        {
            TestDeclaration(@"
                i = 3
                i = 2
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenAssignedInForBody()
        {
            TestDeclaration(@"
                For i = 10 To 24
                    j = 3
                EndFor
            ", @"
                DATAF VI
                DATAF VJ
            ", true);
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenAssignedInWhileBody()
        {
            TestDeclaration(@"
                While 1 = 1
                    j = 3
                EndWhile
            ", @"
                DATAF VJ
            ", true);
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenAssignedInSubroutineBody()
        {
            TestDeclaration(@"
                X()
                Sub X
                    j = 3
                EndSub
            ", @"
                DATAF VJ
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareFloat_WhenAssignedInSubroutineBodyAndNotUsed()
        {
            TestDeclaration(@"
                Sub X
                    j = 3
                EndSub
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldDeclareFloat_WhenAssignedInIfElseBody()
        {
            TestDeclaration(@"
                If Sensor.ReadRawValue(1, 2) = 1 Then
                    i = 1
                ElseIf Sensor.ReadRawValue(1, 2) = 2 Then
                    j = 1
                Else
                    k = 1
                EndIf
            ", @"
                DATAF VI
                DATAF VJ
                DATAF VK
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareFloat_WhenAssignedInIfBodyWithConstantCondition()
        {
            TestDeclaration(@"
                If ""true"" Then
                    i = 1
                ElseIf ""false"" Then
                    j = 1
                Else
                    k = 1
                EndIf
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareFloat_WhenAssignedInIfBodyWithConstantConditionForElseIf()
        {
            TestDeclaration(@"
                If ""false"" Then
                    i = 1
                ElseIf ""true"" Then
                    j = 1
                Else
                    k = 1
                EndIf
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldNotDeclareFloat_WhenAssignedInIfBodyWithConstantConditionInElse()
        {
            TestDeclaration(@"
                If ""false"" Then
                    i = 1
                ElseIf ""false"" Then
                    j = 1
                Else
                    k = 1
                EndIf
            ", @"
            ", true);
        }

        [TestMethod]
        public void ShouldDeclareAllVariables_WhenUsedInThreadSubroutine()
        {
            TestDeclaration(@"
                i = 10
                Thread.Run = THREADA
                Sub THREADA
                    j = i
                EndSub
            ", @"
                DATAF VI
                DATAF VJ
                DATA32 RUNCOUNTER_THREADA
            ", true);
        }

        [TestMethod]
        public void ShouldUseVariable_WhenUsedInThreadSubroutine()
        {
            TestIt(@"
                SMALL = Vector.Init(5, 141)
                Thread.Run = THREADA
                Sub THREADA
                    A = Vector.Sort(5, SMALL)
                EndSub
            ", @"
                SUB_THREADA:
                    CALL VECTOR.SORT 5.0 VSMALL VA
                    SUB8 STACKPOINTER 1 STACKPOINTER
                    READ32 RETURNSTACK STACKPOINTER INDEX
                    JR_DYNAMIC INDEX
                ENDSUB_THREADA:
            ", ExtractSubroutinesCode, true);
        }

    }
}

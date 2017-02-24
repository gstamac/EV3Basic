using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    public class VariableInitializationTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldInitializeFloat()
        {
            TestInitialization(@"
                i = 10.0
            ", @"
                MOVEF_F 0.0 VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeString()
        {
            TestInitialization(@"
                i = ""XX""
            ", @"
                STRINGS DUPLICATE '' VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeFloatArray()
        {
            TestInitialization(@"
                i[2] = 10.1
            ", @"
                CALL ARRAYCREATE_FLOAT VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeStringArray()
        {
            TestInitialization(@"
                i[2] = ""XX""
            ", @"
                CALL ARRAYCREATE_STRING VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeStringArray_WhenUsingCondensedForma()
        {
            TestInitialization(@"
                i = ""1=1;2=2;3=3""
            ", @"
                CALL ARRAYCREATE_STRING VI
            ");
            TestInitialization(@"
                i = ""1=1;2=2;3=3;""
            ", @"
                CALL ARRAYCREATE_STRING VI
            ");
        }
    }
}

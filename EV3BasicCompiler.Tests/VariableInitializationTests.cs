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
            TestIt(@"
                i = 10.0
            ", @"
                MOVEF_F 0.0 VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeString()
        {
            TestIt(@"
                i = ""XX""
            ", @"
                STRINGS DUPLICATE '' VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeFloatArray()
        {
            TestIt(@"
                i[2] = 10.1
            ", @"
                CALL ARRAYCREATE_FLOAT VI
            ");
        }

        [TestMethod]
        public void ShouldInitializeStringArray()
        {
            TestIt(@"
                i[2] = ""XX""
            ", @"
                CALL ARRAYCREATE_STRING VI
            ");
        }

        protected override string CleanupCompiledCode(string ev3Code)
        {
            Match match = Regex.Match(ev3Code, "vmthread MAIN[^{]*{([^}]*)}", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            match = Regex.Match(ev3Code, "ARRAY CREATE8 0 LOCKS([^}]*)ARRAY CREATE8 1 LOCKS", RegexOptions.Singleline);
            if (match.Success)
                ev3Code = match.Groups[1].ToString();
            return ev3Code;
        }
    }
}

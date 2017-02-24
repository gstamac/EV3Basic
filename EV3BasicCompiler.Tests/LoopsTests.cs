using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    [Ignore]
    public class LoopsTests : EV3CompilerTestsBase
    {
        [TestMethod]
        public void ShouldCompileForLoop()
        {
            TestIt(@"
                For i = 10 To 24
                EndFor
            ", @"
                MOVEF_F 10.0 VI              
                for0:                        
                JR_GTF VI 24.0 endfor0       
                forbody0:                    
                ADDF VI 1.0 VI               
                JR_LTEQF VI 24.0 forbody0    
                endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileForLoopWithStep()
        {
            TestIt(@"
                For i = 10 To 24 Step 2
                EndFor
            ", @"
                MOVEF_F 10.0 VI              
                for0:                        
                JR_GTF VI 24.0 endfor0       
                forbody0:                    
                ADDF VI 2.0 VI               
                JR_LTEQF VI 24.0 forbody0    
                endfor0:
            ", ExtractMainProgramCode);
        }

        [TestMethod]
        public void ShouldCompileEndlessWhileLoop()
        {
            TestIt(@"
                While (""true"")
                EndWhile
            ", @"
                while0:          
                whilebody0:      
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
                EndWhile
            ", @"
                MOVEF_F 10.0 VI              
                while0:                      
                JR_LTEQF VI 0.0 endwhile0    
                whilebody0:                  
                JR_GTF VI 0.0 whilebody0     
                endwhile0:
            ", ExtractMainProgramCode);
        }

    }
}

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EV3BasicCompiler.Tests
{
    [TestClass]
    [Ignore]
    public class IntegrationTests : EV3CompilerTestsBase
    {
        const string SOURCE_FILES_DIR = @"C:\Work\GitHub\EV3Basic\Examples";

        [TestMethod]
        public void Test()
        {
            foreach (string fileName in Directory.GetFiles(SOURCE_FILES_DIR, "*.sb"))
            {
                TestFile(fileName, false);
            }
        }

        [TestMethod]
        public void Test_ActionLoop()
        {
            TestFile("ActionLoop.sb", true);
        }

        [TestMethod]
        public void Test_Battery()
        {
            TestFile("Battery.sb", true);
        }

        [TestMethod]
        public void Test_BlockingOperations()
        {
            TestFile("BlockingOperations.sb", true);
        }

        [TestMethod]
        public void Test_BrickBench()
        {
            TestFile("BrickBench.sb", true);
        }

        [TestMethod]
        public void Test_ButtonsAndMotors()
        {
            TestFile("ButtonsAndMotors.sb", true);
        }

        [TestMethod]
        public void Test_ClickTest()
        {
            TestFile("ClickTest.sb", true);
        }

        [TestMethod]
        public void Test_DaisyChain()
        {
            TestFile("DaisyChain.sb", true);
        }

        [TestMethod]
        public void Test_GraphicsAndSounds()
        {
            TestFile("GraphicsAndSounds.sb", true);
        }

        [TestMethod]
        public void Test_HelloWorld()
        {
            TestFile("HelloWorld.sb", true);
        }

        [TestMethod]
        public void Test_MailboxReceive()
        {
            TestFile("MailboxReceive.sb", true);
        }

        [TestMethod]
        public void Test_MailboxSend()
        {
            TestFile("MailboxSend.sb", true);
        }

        [TestMethod]
        public void Test_Melody()
        {
            TestFile("Melody.sb", true);
        }

        [TestMethod]
        public void Test_MotorCount()
        {
            TestFile("MotorCount.sb", true);
        }

        [TestMethod]
        public void Test_MotorSynchronize()
        {
            TestFile("MotorSynchronize.sb", true);
        }

        [TestMethod]
        public void Test_MovingCircle()
        {
            TestFile("MovingCircle.sb", true);
        }

        [TestMethod]
        public void Test_SensorInfo()
        {
            TestFile("SensorInfo.sb", true);
        }

        [TestMethod]
        public void Test_SensorReading()
        {
            TestFile("SensorReading.sb", true);
        }

        [TestMethod]
        public void Test_Threads()
        {
            TestFile("Threads.sb", true);
        }

        [TestMethod]
        public void Test_TimeMeasurement()
        {
            TestFile("TimeMeasurement.sb", true);
        }

        [TestMethod]
        public void Test_TouchSensorTest()
        {
            TestFile("TouchSensorTest.sb", true);
        }

        [TestMethod]
        public void Test_TowersOfHanoi()
        {
            TestFile("TowersOfHanoi.sb", true);
        }

        [TestMethod]
        public void Test_TwoMotorMovement()
        {
            TestFile("TwoMotorMovement.sb", true);
        }

        [TestMethod]
        public void Test_VectorDemo()
        {
            TestFile("VectorDemo.sb", true);
        }

        private void TestFile(string fileName, bool withDump)
        {
            if (!File.Exists(fileName))
                fileName = Path.Combine(SOURCE_FILES_DIR, fileName);

            File.Exists(fileName).Should().BeTrue();

            Console.WriteLine();
            Console.WriteLine($"=============> {Path.GetFileName(fileName)} <=============");
            Console.WriteLine();
            string newCode = Compile(fileName, withDump);
            string oldCode = CompileOld(fileName);

            newCode.Should().Be(oldCode);
        }

        private string Compile(string fileName, bool withDump)
        {
            using (EV3Compiler compiler = new EV3Compiler())
            using (StreamReader stringReader = new StreamReader(fileName))
            using (StringWriter writer = new StringWriter())
            {
                compiler.Parse(stringReader);
                if (withDump)
                    Console.WriteLine(compiler.Dump());

                compiler.GenerateEV3Code(writer);

                compiler.Errors.Should().BeEmpty();

                return CleanupCode(writer.ToString());
            }
        }

        private string CompileOld(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            using (MemoryStream ofs = new MemoryStream())
            {
                List<String> errors = new List<String>();

                EV3BasicCompiler.Compiler c = new EV3BasicCompiler.Compiler();
                c.Compile(fs, ofs, errors);

                errors.Should().BeEmpty();

                return CleanupCode(Encoding.ASCII.GetString(ofs.ToArray()));
            }
        }

    }
}

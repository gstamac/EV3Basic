using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LMSAssembler;
using System.Net.Sockets;
using System.Net;
using EV3BasicCompiler;

namespace Test
{
    class Program
    {
        const String SOURCE_FILENAME = "../../../Examples/SensorReading.sb";
        const string TEST_OUTPUT = "../../../_test/";
        const string TEST_FILES_DIR = "../../../_test";

        static void Main(string[] args)
        {
            //            TestWiFiReceiveBroa dcast();
            //            TestWiFi();
            //TestCompile();
            //TestAssemble();
            //            TestDisassemble();
            CompileAll();

            try
            {
                TestEV3Compiler();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void CompileAll()
        {
            foreach (string fileName in Directory.GetFiles(TEST_FILES_DIR, "*.sb"))
            {
                TestCompile(fileName);
            }
        }

        static void TestEV3Compiler()
        {
            String lmsFilename = Path.Combine(TEST_OUTPUT, Path.GetFileNameWithoutExtension(SOURCE_FILENAME)) + ".lms2";

            using (EV3Compiler compiler = new EV3Compiler())
            using (StreamReader reader = new StreamReader(SOURCE_FILENAME))
            using (StreamWriter writer = new StreamWriter(lmsFilename))
            {
                compiler.Parse(reader);

                compiler.Errors.ForEach(e => Console.WriteLine($"ERROR: {e}"));

                Console.Write(compiler.Dump());

                compiler.GenerateEV3Code(writer);

                writer.WriteLine();
                writer.WriteLine("---------------------------------------------------");
                writer.WriteLine();
                writer.Write(compiler.Dump());
            }
        }

        static void TestDisassemble()
        {
            Assembler a = new Assembler();

            String f = "C:/temp/Program.rbf";
            FileStream fs = new FileStream(f, FileMode.Open, FileAccess.Read);

            a.Disassemble(fs, Console.Out);
        }

        static void TestCompile(string sourceFileName)
        {
            try
            {
                Console.WriteLine("Compiling " + sourceFileName);

                FileStream fs = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
                String lmsFilename = Path.Combine(TEST_OUTPUT, Path.GetFileNameWithoutExtension(sourceFileName)) + ".lms";
                FileStream ofs = new FileStream(lmsFilename, FileMode.Create, FileAccess.Write);

                List<String> errors = new List<String>();

                EV3BasicCompiler.Compiler c = new EV3BasicCompiler.Compiler();
                c.Compile(fs, ofs, errors);

                ofs.Close();
                fs.Close();

                if (errors.Count > 0)
                {
                    foreach (String s in errors)
                    {
                        Console.WriteLine(s);
                    }
                    Console.ReadKey();
                }

                ofs.Close();
                fs.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Compiler error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }

        static void TestAssemble()
        {
            try
            {
                Assembler a = new Assembler();

                String lmsFilename = Path.Combine(TEST_OUTPUT, Path.GetFileNameWithoutExtension(SOURCE_FILENAME)) + ".lms";
                String rmfFilename = Path.Combine(TEST_OUTPUT, Path.GetFileNameWithoutExtension(SOURCE_FILENAME)) + ".rmf";

                FileStream fs = new FileStream(lmsFilename, FileMode.Open, FileAccess.Read);
                FileStream ofs = new FileStream(rmfFilename, FileMode.Create, FileAccess.Write);

                List<String> errors = new List<String>();

                a.Assemble(fs, ofs, errors);

                fs.Close();
                ofs.Close();

                if (errors.Count > 0)
                {
                    foreach (String s in errors)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Assemble error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }


        static void TestWiFi()
        {
            Console.WriteLine("Connecting...");
            TcpClient c = new TcpClient("10.0.0.140", 5555);
            Console.WriteLine("Connected!");
            NetworkStream s = c.GetStream();
            Console.WriteLine("Sending data...");
            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes("GET /target?sn=0016533F0C1E VMTP1.0\r\nProtocol: EV3\r\n\r\n");
            //            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes("X");
            s.Write(data, 0, data.Length);

            for (;;)
            {
                int b = s.ReadByte();
                if (b < 0) break;
                Console.WriteLine(b);
            }

            s.Close();
            c.Close();
        }

        static void TestWiFiReceiveBroadcast()
        {
            Console.WriteLine("Opening receiving UDP port...");
            UdpClient c = new UdpClient(3015);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Receiving incomming packets...");
            for (;;)
            {
                byte[] data = c.Receive(ref RemoteIpEndPoint);
                Console.WriteLine("Received: " + data.Length + "bytes");
                Console.WriteLine(System.Text.UTF8Encoding.UTF8.GetString(data));
            }

            //          c.Close();
        }

    }
}

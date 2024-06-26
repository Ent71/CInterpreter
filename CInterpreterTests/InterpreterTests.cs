using CInterpreter;
using System.Diagnostics.Metrics;

namespace CInterpreterTests
{
    [TestClass]
    public class InterpreterTests
    {
        [TestMethod]
        [DataRow("TestData\\lexer\\test01\\")]
        [DataRow("TestData\\lexer\\test02\\")]
        [DataRow("TestData\\lexer\\test03\\")]
        [DataRow("TestData\\lexer\\test04\\")]
        [DataRow("TestData\\lexer\\test05\\")]
        [DataRow("TestData\\lexer\\test06\\")]
        [DataRow("TestData\\lexer\\test07\\")]
        [DataRow("TestData\\lexer\\test08\\")]
        public void LexerTest(string path)
        {
            int row = 1, column = 1, line = 1;
            bool isWork = true;
            using (StreamReader sr = new StreamReader(Path.Combine(path, "input.c")))
            {
                InterpreterContext context = new InterpreterContext();
                Lexer lexer = new Lexer(context);
                using(StreamWriter writer = new StreamWriter(Path.Combine(path, "actual.txt")))
                {
                    while (isWork)
                    {
                            if (lexer.LexerAnalis(sr, ref row, ref column))
                            {
                                if(lexer.TockenRow.Count() != 0)
                                {
                                    writer.WriteLine("line: {0}", line);
                                }
                                lexer.TockenRowDump(writer);
                                line++;
                            }
                            else
                            {
                                lexer.dumpError(writer);
                                isWork = false;
                            }
                    }
                }
                string expected = File.ReadAllText(Path.Combine(path, "expected.txt")), actual = File.ReadAllText(Path.Combine(path, "actual.txt"));
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("TestData\\parser\\test01\\")]
        [DataRow("TestData\\parser\\test02\\")]
        [DataRow("TestData\\parser\\test03\\")]
        [DataRow("TestData\\parser\\test04\\")]
        [DataRow("TestData\\parser\\test05\\")]
        [DataRow("TestData\\parser\\test06\\")]
        [DataRow("TestData\\parser\\test07\\")]
        [DataRow("TestData\\parser\\test08\\")]
        [DataRow("TestData\\parser\\test09\\")]
        [DataRow("TestData\\parser\\test10\\")]
        [DataRow("TestData\\parser\\test11\\")]
        public void ParserTest(string path)
        {
            int row = 1, column = 1, line = 1;
            bool isWork = true;
            using (StreamReader sr = new StreamReader(Path.Combine(path, "input.c")))
            {
                InterpreterContext context = new InterpreterContext();
                Lexer lexer = new Lexer(context);
                Parser parser = new Parser(context); 
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, "actual.txt")))
                {
                    while (isWork)
                    {
                        if (lexer.LexerAnalis(sr, ref row, ref column))
                        {
                            TreeNode? parserTree = parser.ParseLine(lexer.TockenRow, row);
                            if (parserTree != null)
                            {
                                writer.WriteLine("line: {0}", line);
                                parser.DumpParserTree(parserTree, writer);
                            }
                            else
                            {
                                isWork = false;
                                parser.dumpError(writer);
                            }
                        }
                        else
                        {
                            lexer.dumpError(writer);
                            isWork = false;
                        }
                        line++;
                    }
                }
                string expected = File.ReadAllText(Path.Combine(path, "expected.txt")), actual = File.ReadAllText(Path.Combine(path, "actual.txt"));
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("TestData\\executer\\test01\\")]
        [DataRow("TestData\\executer\\test02\\")]
        [DataRow("TestData\\executer\\test03\\")]
        [DataRow("TestData\\executer\\test04\\")]
        [DataRow("TestData\\executer\\test05\\")]
        [DataRow("TestData\\executer\\test06\\")]
        [DataRow("TestData\\executer\\test07\\")]
        [DataRow("TestData\\executer\\test08\\")]
        [DataRow("TestData\\executer\\test09\\")]
        [DataRow("TestData\\executer\\test10\\")]
        public void ExecuterTest(string path)
        {
            int row = 1, column = 1, line = 1;
            bool isWork = true;
            using (StreamReader sr = new StreamReader(Path.Combine(path, "input.c")))
            {
                InterpreterContext context = new InterpreterContext();
                Lexer lexer = new Lexer(context);
                Parser parser = new Parser(context);
                Executer executer = new Executer(context, new ReadFromConsole(), new WriteToConsole());
                
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, "actual.txt")))
                using (StreamReader reader = new StreamReader(Path.Combine(path, "inputData.txt")))
                {
                    Console.SetIn(reader);
                    Console.SetOut(writer);
                    while (isWork)
                    {
                        if (lexer.LexerAnalis(sr, ref row, ref column))
                        {
                            TreeNode? parserTree = parser.ParseLine(lexer.TockenRow, row);
                            if (parserTree != null)
                            {
                                if(!executer.ExecuteProgram(parserTree))
                                {
                                    isWork = false;
                                    executer.dumpError(writer);
                                }
                            }
                            else
                            {
                                isWork = false;
                                parser.dumpError(writer);
                            }
                        }
                        else
                        {
                            lexer.dumpError(writer);
                            isWork = false;
                        }
                        line++;
                    }
                }
                
                Console.SetIn(Console.In);
                Console.SetOut(Console.Out);
                string expected = File.ReadAllText(Path.Combine(path, "expected.txt")), actual = File.ReadAllText(Path.Combine(path, "actual.txt"));
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
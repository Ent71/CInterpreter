using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
 
namespace CInterpreter
{
    public class Interpreter
    {
        public Interpreter(ILexer lexer, IParser parser, IExecuter executer) 
        {
            this.lexer = lexer;
            this.parser = parser;
            this.executer = executer;
        }



        public void Run(StreamReader sr)
        {
            int row = 1, column = 1, line = 1;
            bool isWork = true;
            while (isWork)
            {
                if(lexer.LexerAnalis(sr, ref row, ref column))
                {
                    //lexer.TockenRowDump(Console.Out);
                    TreeNode? parserTree = parser.ParseLine(lexer.TockenRow, row);
                    if (parserTree != null)
                    {
                        Console.WriteLine("line: {0}", line);
                        parser.DumpParserTree(parserTree, Console.Out);
                        //isWork = executer.ExecuteProgram(parserTree, row);
                    }
                    else
                    {
                        isWork = false;
                    }

                    line++;
                }
                else
                {
                    lexer.dumpError(Console.Out);
                    isWork = false;
                }
            }
        }

        private ILexer lexer;
        private IParser parser;
        private IExecuter executer;
    }
}

using CInterpreter.Models;
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
        private ILexer lexer;
        private IParser parser;
        private IExecuter executer;

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
                    TreeNode? parserTree = parser.ParseLine(lexer.TockenRow, row);
                    if (parserTree != null)
                    {
                        if (!executer.ExecuteProgram(parserTree))
                        {
                            isWork = false;
                            executer.dumpError(Console.Out);
                        }
                    }
                    else
                    {
                        parser.dumpError(Console.Out);
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

            lexer.Reset();
            parser.Reset();
            executer.Reset();
        }
    }
}

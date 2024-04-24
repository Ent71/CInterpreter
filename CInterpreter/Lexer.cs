using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CInterpreter.Models;

namespace CInterpreter
{
    public class Lexer : ILexer
    {
        private string errorMessage = "";
        private List<Tocken> tockenRow = new List<Tocken>();
        private InterpreterContext context;
        private int identifierID = 1;

        public IReadOnlyList<Tocken> TockenRow { get { return tockenRow; } }

        public Lexer(InterpreterContext context)
        {
            this.context = context;
        }

        public void Reset()
        {
            errorMessage = "";
            tockenRow.Clear();
            context.Reset();
            identifierID = 1;
        }

        public bool LexerAnalis(StreamReader sr, ref int row, ref int column)
        {
            tockenRow.Clear();
            int startColumn = column;
            StringBuilder sb = new StringBuilder(15);
            Symbol symbol = new Symbol();
            bool isWorkLine = symbol.Read(sr);
            if(symbol.endOfFile)
            {
                return false;
            }
            
            while (isWorkLine && !symbol.endOfFile)
            {
                sb.Clear();

                if (char.IsDigit(symbol.Character))
                {
                    startColumn = column;
                    sb.Append(symbol.Character);
                    while (symbol.Read(sr) && char.IsDigit(symbol.Character))
                    {
                        column++;
                        sb.Append(symbol.Character);
                    }
                    tockenRow.Add(new DigitTocken(Int32.Parse(sb.ToString()), row, startColumn));
                }
                else if (char.IsLetter(symbol.Character))
                {
                    startColumn = column;
                    sb.Append(symbol.Character);
                    while (symbol.Read(sr) && (char.IsDigit(symbol.Character) || char.IsLetter(symbol.Character)))
                    {
                        column++;
                        sb.Append(symbol.Character);
                    }

                    string str = sb.ToString();
                    if (context.keyWordTable.ContainsKey(str))
                    {
                        tockenRow.Add(new KeyWordTocken(context.keyWordTable[str], row, startColumn));
                    }
                    else
                    {
                        if (context.identifierToId.ContainsKey(str))
                        {
                            tockenRow.Add(new IdentifierTocken(context.identifierToId[str], row, startColumn));
                        }
                        else
                        {
                            context.identifierToId[str] = identifierID;
                            context.idToIdentifier[identifierID] = str;
                            tockenRow.Add(new IdentifierTocken(identifierID, row, startColumn));
                            ++identifierID;
                        }

                    }


                }
                else if (symbol.Character == '\"')
                {
                    bool inStr = true;
                    startColumn = column;
                    while (inStr)
                    {
                        if (symbol.Read(sr) && symbol.Character != '\n')
                        {
                            column++;
                            if (symbol.Character == '\"')
                            {
                                tockenRow.Add(new StringTocken(sb.ToString(), row, startColumn));
                                inStr = false;
                            }
                            else
                            {
                                sb.Append(symbol.Character);
                            }
                        }
                        else
                        {
                            SaveLexerError(string.Format("Not closed '\"'"), row, startColumn);
                            return false;
                        }
                    }
                    symbol.Read(sr);
                }
                else if (symbol.Character == '=')
                {
                    tockenRow.Add(new AssigmentTocken(row, column));
                    symbol.Read(sr);
                }
                else if (symbol.Character == ',' || symbol.Character == ';' || symbol.Character == '(' || symbol.Character == ')')
                {
                    tockenRow.Add(new DelimeterTocken(symbol.Character, row, column));
                    if (symbol.Character == ';')
                    {
                        isWorkLine = false;
                    }
                    symbol.Read(sr);
                }
                else if (IsArithmeticOperation(symbol.Character))
                {
                    if (symbol.Character == '/')
                    {
                        if (symbol.Read(sr) && symbol.Character == '/')
                        {
                            context.endOfRowPositions.Add(column - 1);
                            do
                            {
                            } while (symbol.Read(sr) && symbol.Character != '\n');
                            column = 0;
                            row++;
                        }
                        else
                        {
                            tockenRow.Add(new ArithmeticTocken('/', row, column));
                        }
                    }
                    else
                    {
                        tockenRow.Add(new ArithmeticTocken(symbol.Character, row, column));
                        symbol.Read(sr);
                    }
                }
                else if (char.IsWhiteSpace(symbol.Character))
                {
                    if (symbol.Character == '\n')
                    {
                        context.endOfRowPositions.Add(column);
                        row++;
                        column = 0;
                    }
                    symbol.Read(sr);
                }
                else
                {
                    SaveLexerError(string.Format("Unexpected character: \'{0}\'", symbol.Character), row, column);
                    return false;
                }


                column++;
            }
            return true;
        }
        public void TockenRowDump(TextWriter output)
        {
            foreach (Tocken tocken in tockenRow)
            {
                context.DumpTocken(tocken, output);
                output.WriteLine();
            }
        }

        private void SaveLexerError(string message, int row, int column)
        {
            errorMessage = string.Format("Lexer Error: {0} line: {1}, column: {2}", message, row, column);
        }

        public void dumpError(TextWriter output)
        {
            if(errorMessage.Length != 0)
            {
                output.WriteLine(errorMessage);
            }
        }

        public static bool IsArithmeticOperation(char c)
        {
            return c == '-' || c == '+' || c == '*' || c == '/';
        }
    }
}
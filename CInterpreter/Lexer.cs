using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    public class Lexer : ILexer
    {
        public Lexer(InterpreterContext context)
        {
            this.context = context;
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
                    // column++;
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
                    // column++;
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
                else if (symbol.Character == '\"') // TODO: \n
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
                            inStr = false;
                            errorMessage = string.Format("Lexer Error: Not closed \'\"\' line: {1}, column: {2}", symbol.Character, row, startColumn);
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
                        row++;
                        column = 0;
                    }
                    symbol.Read(sr);
                }
                else
                {
                    errorMessage = string.Format("Lexer Error: Unexpected character: \'{0}\' line: {1}, column: {2}", symbol.Character, row, column);
                    return false;
                }


                column++;
            }
            return true;
        }

        //public void TockenRowDump(int line, StreamWriter output)
        //{
        //    TockenRowDump(line, output);
        //}
        public void TockenRowDump(TextWriter output)
        {
            foreach (Tocken tocken in tockenRow)
            {
                context.DumpTocken(tocken, output);
                output.WriteLine();
            }
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
        private string errorMessage = "";
        public IReadOnlyList<Tocken> TockenRow { get { return tockenRow; } }

        private List<Tocken> tockenRow = new List<Tocken>();

        private InterpreterContext context;

        private int identifierID = 1;
    }
}
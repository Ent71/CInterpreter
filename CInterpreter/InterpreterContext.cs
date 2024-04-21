using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CInterpreter.Lexer;

namespace CInterpreter
{
    
    public class InterpreterContext
    {
        public void DumpTocken(Tocken tocken, TextWriter output)
        {
            output.Write("row: {0}, column: {1} ", tocken.row, tocken.column);
            switch (tocken)
            {
                case StringTocken strTocken:
                    output.Write("val: {0}", strTocken.stringValue);
                    break;
                case DigitTocken dgtTocken:
                    output.Write("val: {0}", dgtTocken.digitValue);
                    break;
                case DelimeterTocken dlmtTocken:
                    output.Write("val: {0}", dlmtTocken.delimeter);
                    break;
                case ArithmeticTocken arithTocken:
                    output.Write("val: {0}", arithTocken.operation);
                    break;
                case IdentifierTocken idTocken:
                    output.Write("val: {0}", idToIdentifier[idTocken.ID]);
                    break;
                case KeyWordTocken keyWordTocken:
                    output.Write("val: {0}", keyWordTocken.ID);
                    break;
                case AssigmentTocken:
                    output.Write("val: =");
                    break;
                case ParserTocken parserTocken:
                    output.Write("val: {0}", parserTocken.parseTocken);
                    break;
            }
        }

        public Dictionary<string, KeyWordTocken.KeyWordID> keyWordTable = new Dictionary<string, KeyWordTocken.KeyWordID> { { "int", KeyWordTocken.KeyWordID.Int }, 
                                                                                                                            { "Read", KeyWordTocken.KeyWordID.Read }, 
                                                                                                                            { "Write", KeyWordTocken.KeyWordID.Write } };
        public Dictionary<string, int> identifierToId = new Dictionary<string, int>();
        public Dictionary<int, string> idToIdentifier = new Dictionary<int, string>();
    }
}

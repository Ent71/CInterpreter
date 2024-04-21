using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CInterpreter.Interpreter;

namespace CInterpreter
{
    public interface ILexer
    {
        public bool LexerAnalis(StreamReader reader, ref int row, ref int column);
        public void TockenRowDump(TextWriter output);
        public void dumpError(TextWriter output);

        public IReadOnlyList<Tocken> TockenRow { get; }
    };
}

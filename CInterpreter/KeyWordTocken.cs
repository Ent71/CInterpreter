using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CInterpreter.Interpreter;

namespace CInterpreter
{
    public class KeyWordTocken : Tocken
    {
        public KeyWordTocken(KeyWordID val, int row, int column)
            : base(row, column)
        {
            ID = val;
        }

        public enum KeyWordID
        {
            Int,
            Read,
            Write
        }

        // public string StringValue { get; }
        public readonly KeyWordID ID;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class DigitTocken : Tocken
    {
        public DigitTocken(int val, int row, int column) 
            : base(row, column)
        {
            digitValue = val;
        }

        // public int DigitValue { get; }
        public readonly int digitValue;
    }
}

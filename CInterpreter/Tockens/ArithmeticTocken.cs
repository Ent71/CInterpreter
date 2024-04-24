using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class ArithmeticTocken : Tocken
    {
        public ArithmeticTocken(char val, int row, int column) 
            :base(row, column)
        {
            operation = val;
        }

        public readonly char operation;
    }
}
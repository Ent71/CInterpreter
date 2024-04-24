using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class DelimeterTocken : Tocken
    {
        public DelimeterTocken(char val, int row, int column)
            : base(row, column)
        {
            delimeter = val;
        }

        public readonly char delimeter;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    class StringTocken : Tocken
    {
        public StringTocken(string val, int row, int column) 
            : base(row, column)
        {
            stringValue = val;
        }

        public readonly string stringValue;
    }
}
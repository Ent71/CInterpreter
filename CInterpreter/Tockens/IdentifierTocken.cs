using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class IdentifierTocken : Tocken
    {
        public IdentifierTocken(int val,int row, int column)
            :base(row, column)
        {
            ID = val;
        }

        public readonly int ID;
    }
}
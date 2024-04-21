using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class AssigmentTocken : Tocken
    {
        public AssigmentTocken(int row, int column)
            : base(row, column) { }
    }
}

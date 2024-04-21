using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    public class Tocken
    {
        public Tocken() {}

        public Tocken(int row, int column)
        {
            this.row = row;
            this.column = column;
            //this.id = id;
        }
        //public int ID { get; }

        public readonly int row, column;
    }
}

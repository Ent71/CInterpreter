using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class Symbol
    {
        public bool Read(StreamReader sr)
        {
            int charValue = sr.Read();
            if (charValue == -1) 
            {
                endOfFile = true;
                return false;
            }
            Character = (char)charValue;
            return true;
        }

        public bool endOfFile { get; set; } = false;

        public char Character { get; set; }
    }
}

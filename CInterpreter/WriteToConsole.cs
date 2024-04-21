using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class WriteToConsole : IWritable
    {
        public void Write(string data) 
        {
            Console.WriteLine(data);
        }
    }
}
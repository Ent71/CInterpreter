using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CInterpreter.Models;

namespace CInterpreter
{
    public class WriteToConsole : IWritable
    {
        public void Write(string data) 
        {
            Console.WriteLine(data);
        }
    }
}
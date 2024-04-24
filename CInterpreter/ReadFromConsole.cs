using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CInterpreter.Models;

namespace CInterpreter
{
    public class ReadFromConsole : IReadable
    {
        public int? Read()
        {
            string? value = Console.ReadLine();
            if (value != null && int.TryParse((string)value, out int result))
            {
                return result;
            }
            return null;
        }
    }
}

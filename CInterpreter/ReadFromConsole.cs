using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class ReadFromConsole : IReadable
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

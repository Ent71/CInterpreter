using System.Threading.Channels;

namespace CInterpreter
{
    class Program
    {
        static StreamReader? FileOpener(string path)
        {
            try
            {
                return new StreamReader(path);
            }
            catch (FileNotFoundException) 
            {
                Console.WriteLine("File not found");
                return null;
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Input file name:");
            string? fileName = Console.ReadLine(); 
            InterpreterContext context = new InterpreterContext();
            Interpreter intr = new Interpreter(new Lexer(context), new Parser(context), new Executer(context , new ReadFromConsole(), new WriteToConsole()));

            if (fileName != null)
            {
                using (StreamReader? sr = FileOpener((string)fileName))
                {
                    if(sr != null)
                    {
                        Stream outputStream = Console.OpenStandardOutput();
                        intr.Run(sr);
                    }
                    else
                    {
                        Console.WriteLine("File not found");
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}

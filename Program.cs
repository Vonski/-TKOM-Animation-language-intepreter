using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Code
{
    class Program
    {
        static void Main(string[] args)
        {
            // Lexer lexer = new Lexer("./testCode.txt");
            // TokenInfo token = lexer.NextToken();
            // while(token.token!=TokenType.EOF)
            // {
            //     Console.WriteLine("{ " + token.token + ", " + token.code + ", " + token.value +  ", " + token.line +  ", " + token.position + "}");
            //     token = lexer.NextToken();
            // }
            Parser parser = new Parser("./testCode.txt");
            List<Statement> list = parser.Run();
            Console.WriteLine("Start petli:");
            foreach(Statement stat in list)
            {
                Console.WriteLine("{ " + stat + " }");
            }
        }
    }
}

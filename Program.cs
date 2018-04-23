using System;
using System.IO;
using System.Text;
//using Code;

namespace Code
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = File.ReadAllText("./testCode.txt");
            Lexer lexer = new Lexer(source);
            TokenInfo token = lexer.NextToken();
            while(token.token!=TokenType.EOF)
            {
                Console.WriteLine("{ " + token.token + ", " + token.code + "}");
                token =lexer.NextToken();
            }
        }
    }
}

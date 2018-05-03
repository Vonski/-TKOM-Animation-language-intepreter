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
            Lexer lexer = new Lexer("./testCode.txt");
            TokenInfo token = lexer.NextToken();
            while(token.token!=TokenType.EOF)
            {
                Console.WriteLine("{ " + token.token + ", " + token.code + ", " + token.value +  ", " + token.line +  ", " + token.position + "}");
                token = lexer.NextToken();
            }

            // Scanner scan = new Scanner("./testCode.txt");
            // char? ch;
            // for (int i=0; i<520; ++i)
            //     if ((ch = scan.GetNextChar())!=null)
            //         Console.Write(ch);
        }
    }
}

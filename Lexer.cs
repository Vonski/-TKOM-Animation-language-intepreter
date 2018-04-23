using System;
using System.Collections.Generic;

namespace Code
{
    enum TokenType 
    {
        Dot,
        AssignmentOperator,
        Figure,
        CurlyBracketLeft,
        CurlyBracketRight,
        Animation,
        ParenthesisLeft,
        ParenthesisRight,
        Comma,
        Semicolon,
        If,
        Else,
        For,
        In,
        Each,
        EqualityOperator,
        InequalityOperator,
        GreaterOperator,
        GreaterOrEqualOperator,
        LessOperator,
        LessOrEqualOperator,
        PlusOperator,
        MinusOperator,
        AsteriskOperator,
        SlashOperator,
        Collection,
        SquareBracketLeft,
        SquareBracketRight,
        Hash,
        EOF
    }
    struct TokenInfo
    {
        public TokenInfo(TokenType tt, string c)
        {
            token = tt;
            code = c;
        }
        public TokenType token;
        public string code;
    }
    class Lexer
    {
        public Lexer(string src)
        {
            source = src;
            posInSource = 0;
            lineCounter = 1;
            keyWords = new List<string>(new string[] {"figure", "animation", "if", "else", "for", "each", "in", "collection"});
        }

        public TokenInfo NextToken()
        {
            if(posInSource>=source.Length)
                return new TokenInfo(TokenType.EOF, "");
            while(Char.IsWhiteSpace(source[posInSource]))
                if(!NextChar())
                    return new TokenInfo(TokenType.EOF, "");

            switch(source[posInSource])
            {
                case char ch when Char.IsLetter(ch):
                    // handle id and keywords
                    break;
                case char ch when Char.IsDigit(ch):
                    // handle number literals
                    break;
                case '#':
                    // handle color hashes
                    break;
                case '=':
                    // handle assignment and equality
                    break;
                case '>':
                    // handle grt, gre
                    break;
                case '<':
                    // handle lst, lse
                    break;
                case '!':
                    // handle inequality
                    break;
                case '{': NextChar(); return new TokenInfo(TokenType.CurlyBracketLeft, "{");
                case '}': NextChar(); return new TokenInfo(TokenType.CurlyBracketRight, "}");
                case '(': NextChar(); return new TokenInfo(TokenType.ParenthesisLeft, "(");
                case ')': NextChar(); return new TokenInfo(TokenType.ParenthesisRight, ")");
                case '[': NextChar(); return new TokenInfo(TokenType.SquareBracketLeft, "[");
                case ']': NextChar(); return new TokenInfo(TokenType.SquareBracketRight, "]");
                case '+': NextChar(); return new TokenInfo(TokenType.PlusOperator, "+");
                case '-': NextChar(); return new TokenInfo(TokenType.MinusOperator, "-");
                case '*': NextChar(); return new TokenInfo(TokenType.AsteriskOperator, "*");
                case '/': NextChar(); return new TokenInfo(TokenType.SlashOperator, "/");
                case '.': NextChar(); return new TokenInfo(TokenType.Dot, ".");
                case ',': NextChar(); return new TokenInfo(TokenType.Comma, ",");
                case ';': NextChar(); return new TokenInfo(TokenType.Semicolon, ";");
                default:
                    return new TokenInfo(TokenType.EOF, "");
                    //break;
            }

            return new TokenInfo(TokenType.EOF, "");
        }

        private bool NextChar()
        {
            posInSource++;
            if (source.Length>posInSource)
            {
                if(source[posInSource]=='\n')
                    lineCounter++;
                return true;
            }
            return false;
        }
        private string source;
        private int posInSource;
        private int lineCounter;
        private List<string> keyWords;
    }
}




// Easter eggs and sausage

// ****
//     ****
//         ****
//             ***
//             ***
//             ***
//         ****
//     ****
//         ------------------------------------------------\
//                                                     |    -\
//                                                     |  ____|)
//                                                     |      |)
//                                                     |    -/
//         ------------------------------------------------/
//     ****
//         ****
//             ***
//             ***
//             ***
//         ****
//     **** 
// ****
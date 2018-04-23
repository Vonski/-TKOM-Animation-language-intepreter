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
        Integer,
        Color,
        Identifier,
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
            try
            { 
                switch(source[posInSource])
                {
                    case char ch when Char.IsLetter(ch):
                        string id = "";
                        while(char.IsLetterOrDigit(source[posInSource]))
                        {
                            id += source[posInSource];
                            if(!NextChar())
                                return new TokenInfo(TokenType.Integer, id);
                            if(id.Length>MAX_ID)
                                throw new LexicalException("Too long identifier, in line " + lineCounter);
                        }
                        int num = keyWords.FindIndex(word => word == id);
                        if (num!=-1)
                            return new TokenInfo(GetDictionary()[num], id);
                        else
                            return new TokenInfo(TokenType.Identifier, id);
                    case char ch when Char.IsDigit(ch):
                        if (source[posInSource]=='0')
                                throw new LexicalException("Integer can't start from 0, in line " + lineCounter);
                        string number = "";
                        while (char.IsDigit(source[posInSource]))
                        {
                            number += source[posInSource];
                            if(!NextChar())
                                return new TokenInfo(TokenType.Integer, number);
                        }
                        return new TokenInfo(TokenType.Integer, number);
                    case '#':
                        string color = "#";
                        for(int i=0; i<6; ++i)
                        {
                            if(!NextChar())
                                throw new LexicalException("Unexpected end of file, in line " + lineCounter);
                            if(!Uri.IsHexDigit(source[posInSource]))
                                throw new LexicalException("Color should be typed like these # and 6*hexdigit, in line " + lineCounter);
                            color += source[posInSource];
                        }
                        NextChar();
                        return new TokenInfo(TokenType.Color, color);
                    case '=':
                        return HandleTwoCharOps(TokenType.AssignmentOperator, TokenType.EqualityOperator, '=');
                    case '>':
                        return HandleTwoCharOps(TokenType.GreaterOperator, TokenType.GreaterOrEqualOperator, '>');
                    case '<':
                        return HandleTwoCharOps(TokenType.LessOperator, TokenType.LessOrEqualOperator, '<');
                    case '!':
                        if(!NextChar() || source[posInSource]!='=')
                            throw new LexicalException("Bad operator '!=', in line " + lineCounter);
                        NextChar();
                        return new TokenInfo(TokenType.InequalityOperator, "!=");
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
                    default: throw new LexicalException("Unknown statement, in line " + lineCounter);
                }
            }
            catch(LexicalException le)
            {
                throw le;
            }
        }

        private Dictionary<int, TokenType> GetDictionary()
        {
                    var dictionary = new Dictionary<int, TokenType>();
                    dictionary[0] = TokenType.Figure;
                    dictionary[1] = TokenType.Animation;
                    dictionary[2] = TokenType.If;
                    dictionary[3] = TokenType.Else;
                    dictionary[4] = TokenType.For;
                    dictionary[5] = TokenType.Each;
                    dictionary[6] = TokenType.In;
                    dictionary[7] = TokenType.Collection;
                    return dictionary;
        }

        private TokenInfo HandleTwoCharOps(TokenType t1, TokenType t2, char ch)
        {
            if(!NextChar())
                return new TokenInfo(t1, ch.ToString());
            if(source[posInSource]=='=')
            {
                NextChar();
                return new TokenInfo(t2, ch+"=");
            }
            else
                return new TokenInfo(t1, ch.ToString());
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
        private const int MAX_ID = 64;
        private List<string> keyWords;
    }

    class LexicalException : Exception
    {
        public LexicalException() {}
        public LexicalException(string message) : base(message) { Console.WriteLine(message); }
        public LexicalException(string message, Exception inner) : base(message,inner) { Console.WriteLine(message); }
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
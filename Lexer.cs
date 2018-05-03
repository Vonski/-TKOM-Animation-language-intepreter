using System;
using System.Collections.Generic;

namespace Code
{
    class Lexer
    {
        public Lexer(string path)
        {
            scanner = new Scanner(path);
            posInSource = -1;
            currentChar = NextChar();
            lineCounter = 1;
            dictionary = new Dictionary<string, TokenType>();
            dictionary["figure"] = TokenType.Figure;
            dictionary["animation"] = TokenType.Animation;
            dictionary["if"] = TokenType.If;
            dictionary["else"] = TokenType.Else;
            dictionary["for"] = TokenType.For;
            dictionary["each"] = TokenType.Each;
            dictionary["in"] = TokenType.In;
            dictionary["collection"] = TokenType.Collection;
        }

        public TokenInfo NextToken()
        {
            if(currentChar==null)
                return new TokenInfo(TokenType.EOF, "", posInSource, lineCounter);
            while(Char.IsWhiteSpace(currentChar ?? default(char)))
            {
                currentChar = NextChar();
                if(currentChar==null)
                    return new TokenInfo(TokenType.EOF, "", posInSource, lineCounter);
            }
            
            currentTokenPosition = posInSource;

            switch(currentChar)
            {
                case char ch when Char.IsLetter(ch): return HandleKeyWordsAndIdentifiers();
                case char ch when Char.IsDigit(ch): return HandleNumbers();
                case '#': return HandleColors();
                case '=': return HandleTwoCharOps(TokenType.AssignmentOperator, TokenType.EqualityOperator, '=');
                case '>': return HandleTwoCharOps(TokenType.GreaterOperator, TokenType.GreaterOrEqualOperator, '>');
                case '<': return HandleTwoCharOps(TokenType.LessOperator, TokenType.LessOrEqualOperator, '<');
                case '!': return HandleInequalityOp();
                case '{': currentChar = NextChar(); return new TokenInfo(TokenType.CurlyBracketLeft, "{", currentTokenPosition, lineCounter);
                case '}': currentChar = NextChar(); return new TokenInfo(TokenType.CurlyBracketRight, "}", currentTokenPosition, lineCounter);
                case '(': currentChar = NextChar(); return new TokenInfo(TokenType.ParenthesisLeft, "(", currentTokenPosition, lineCounter);
                case ')': currentChar = NextChar(); return new TokenInfo(TokenType.ParenthesisRight, ")", currentTokenPosition, lineCounter);
                case '[': currentChar = NextChar(); return new TokenInfo(TokenType.SquareBracketLeft, "[", currentTokenPosition, lineCounter);
                case ']': currentChar = NextChar(); return new TokenInfo(TokenType.SquareBracketRight, "]", currentTokenPosition, lineCounter);
                case '+': currentChar = NextChar(); return new TokenInfo(TokenType.PlusOperator, "+", currentTokenPosition, lineCounter);
                case '-': currentChar = NextChar(); return new TokenInfo(TokenType.MinusOperator, "-", currentTokenPosition, lineCounter);
                case '*': currentChar = NextChar(); return new TokenInfo(TokenType.AsteriskOperator, "*", currentTokenPosition, lineCounter);
                case '/': currentChar = NextChar(); return new TokenInfo(TokenType.SlashOperator, "/", currentTokenPosition, lineCounter);
                case '.': currentChar = NextChar(); return new TokenInfo(TokenType.Dot, ".", currentTokenPosition, lineCounter);
                case ',': currentChar = NextChar(); return new TokenInfo(TokenType.Comma, ",", currentTokenPosition, lineCounter);
                case ';': currentChar = NextChar(); return new TokenInfo(TokenType.Semicolon, ";", currentTokenPosition, lineCounter);
                default: throw new LexicalException("Unknown statement, in line " + lineCounter);
            }
        }

        private TokenInfo HandleKeyWordsAndIdentifiers()
        {
            string id = "";
            while(char.IsLetterOrDigit(currentChar ?? default(char)))
            {
                id += currentChar;
                currentChar = NextChar();
                if(id.Length>MAX_ID)
                    throw new LexicalException("Too long identifier, in line " + lineCounter);
            }
            if (dictionary.ContainsKey(id))
                return new TokenInfo(dictionary[id], id, currentTokenPosition, lineCounter);
            else
                return new TokenInfo(TokenType.Identifier, id, currentTokenPosition, lineCounter);
        }

        private TokenInfo HandleNumbers()
        {
            if (currentChar=='0')
            {
                currentChar = NextChar();
                if (!char.IsDigit(currentChar ?? default(char)))
                    return new TokenInfo(TokenType.Integer, 0, currentTokenPosition, lineCounter);
                else
                    throw new LexicalException("Integer can't start from 0, in line " + lineCounter);
            }
            string number = "";
            while (char.IsDigit(currentChar ?? default(char)))
            {
                number += currentChar;
                currentChar = NextChar();
            }
            return new TokenInfo(TokenType.Integer, BuildInteger(number), currentTokenPosition, lineCounter);
        }

        private TokenInfo HandleColors()
        {
            string color = "#";
            for(int i=0; i<6; ++i)
            {
                currentChar = NextChar();
                if(!Uri.IsHexDigit(currentChar ?? default(char)))
                    throw new LexicalException("Color should be typed like these # and 6*hexdigit, in line " + lineCounter);
                color += currentChar;
            }
            currentChar = NextChar();
            return new TokenInfo(TokenType.Color, color, currentTokenPosition, lineCounter);
        }

        private TokenInfo HandleTwoCharOps(TokenType t1, TokenType t2, char ch)
        {
            currentChar = NextChar();
            if(currentChar=='=')
            {
                currentChar = NextChar();
                return new TokenInfo(t2, ch+"=", currentTokenPosition, lineCounter);
            }
            else
                return new TokenInfo(t1, ch.ToString(), currentTokenPosition, lineCounter);
        }

        private TokenInfo HandleInequalityOp()
        {
            currentChar = NextChar();
            if(currentChar!='=')
                throw new LexicalException("Bad operator '!=', in line " + lineCounter);
            currentChar = NextChar();
            return new TokenInfo(TokenType.InequalityOperator, "!=", currentTokenPosition, lineCounter);
        }

        private char? NextChar()
        {
            posInSource++;
            char? ch = scanner.GetNextChar();
            if(ch=='\n')
                lineCounter++;
            return ch;
        }

        private int BuildInteger(string num)
        {
            int ret = 0;
            foreach(char ch in num)
            {
                ret = ret * 10 + (int)char.GetNumericValue(ch);
            }
            return ret;
        }

        char? currentChar;
        private int currentTokenPosition;
        private Scanner scanner;
        private int posInSource;
        private int lineCounter;
        private const int MAX_ID = 64;
        private Dictionary<string, TokenType> dictionary;
    }

    class LexicalException : Exception
    {
        public LexicalException() {}
        public LexicalException(string message) : base(message) { Console.WriteLine(message); }
        public LexicalException(string message, Exception inner) : base(message,inner) { Console.WriteLine(message); }
    }
}
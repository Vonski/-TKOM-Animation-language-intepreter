using System;

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
        public TokenInfo(TokenType tt, string c, int pos, int l)
        {
            token = tt;
            code = c;
            position = pos;
            line = l;
            value = null;
        }
        public TokenInfo(TokenType tt, int v, int pos, int l)
        {
            token = tt;
            code = null;
            position = pos;
            line = l;
            value = v;
        }
        public TokenType token;
        public string code;
        public int? value;
        public int position;
        public int line;
    }
}
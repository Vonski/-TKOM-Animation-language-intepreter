using System;
using System.Collections.Generic;

namespace Code
{
    class Parser
    {
        public Parser(string path)
        {
            lexer = new Lexer(path);
            statements = new Queue<Statement>();
            currentToken = lexer.NextToken();
        }

        public Queue<Statement> Run()
        {
            while(currentToken.token!=TokenType.EOF)
            {
                try
                {
                    statements.Enqueue(NextStatement());
                }
                catch { return statements; }
            }
            return statements;
        }

        private void NextToken()
        {
            currentToken = lexer.NextToken();
        }

        private bool Accept(TokenType symbol)
        {
            if (currentToken.token == symbol)
            {
                NextToken();
                return true;
            }
            return false;
        }

        private bool Expect(TokenType symbol)
        {
            return Expect(symbol, symbol + " symbol expected in line " + currentToken.line);
        }

        private bool Expect(TokenType symbol, string msg)
        {
            if (currentToken.token == symbol)
            {
                NextToken();
                return true;
            }
            throw new SyntaxException(msg);
        }

        private ValueEvaluator GetValueEvaluator(TokenInfo token)
        {
            List<Identifier> ids = new List<Identifier>();
            ids.Add(new Identifier(token.code, 0));
            ids = GetAttribToken(ids);
            return new ValueEvaluator(ids);
        }

        private Statement NextStatement()
        {
            Statement ret=null;
            ret = HandleFigureDeclaration();
            if (ret!=null)
                return ret;
            ret = HandleAnimationDeclaration();
            if (ret!=null)
                return ret;
            ret = HandleIfStatement();
            if (ret!=null)
                return ret;
            ret = HandleForEachStatement();
            if (ret!=null)
                return ret;
            ret = HandleEachStatement();
            if (ret!=null)
                return ret;
            var token = currentToken;
            Expect(TokenType.Identifier, "Unexpected symbol in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(token);
            if (ret!=null)
                return ret;
            var value = GetValueEvaluator(token);
            ret = HandleAnimationCall(value);
            if (ret!=null)
                return ret;
            ret = HandleAssignment(value);
            if (ret!=null)
                return ret;
            throw new SyntaxException("Unexpected symbol in line: " + currentToken.line);
        }

        private Statement NextDeclStatement()
        {
            Statement ret=null;
            ret = HandleFigureDeclaration();
            if (ret!=null)
                return ret;
            ret = HandleAnimationDeclaration();
            if (ret!=null)
                return ret;
            var token = currentToken;
            Expect(TokenType.Identifier, "Unexpected symbol in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(token);
            if (ret!=null)
                return ret;
            var value = GetValueEvaluator(token);
            ret = HandleAssignment(value);
            if (ret!=null)
                return ret;
            if(Accept(TokenType.EOF))
                throw new SyntaxException("Unexpected end of file");
            throw new SyntaxException("Unexpected symbol in declaration in line: " + currentToken.line);
        }

        private Queue<Statement> GetDeclStatementBlock()
        {
            Queue<Statement> ret = new Queue<Statement>();
            while(!Accept(TokenType.CurlyBracketRight))
                ret.Enqueue(NextDeclStatement());
            return ret;
        }

        private Statement NextBlockStatement()
        {
            Statement ret=null;
            ret = HandleIfStatement();
            if (ret!=null)
                return ret;
            ret = HandleForEachStatement();
            if (ret!=null)
                return ret;
            var token = currentToken;
            Expect(TokenType.Identifier, "Unexpected symbol in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(token);
            if (ret!=null)
                return ret;
            var value = GetValueEvaluator(token);
            ret = HandleAnimationCall(value);
            if (ret!=null)
                return ret;
            ret = HandleAssignment(value);
            if (ret!=null)
                return ret;
            if(Accept(TokenType.EOF))
                throw new SyntaxException("Unexpected end of file");
            throw new SyntaxException("Unexpected symbol in line: " + currentToken.line);
        }

        private Queue<Statement> GetStatementBlockInCurlyBracket()
        {
            Queue<Statement> ret = new Queue<Statement>();
            while(!Accept(TokenType.CurlyBracketRight))
                ret.Enqueue(NextBlockStatement());
            return ret;
        }

        private Queue<Statement> GetStatementBlock()
        {
            var ret = new Queue<Statement>();
            if(Accept(TokenType.CurlyBracketLeft))
            {
                while(!Accept(TokenType.CurlyBracketRight))
                    ret.Enqueue(NextBlockStatement());
            }
            else
                ret.Enqueue(NextBlockStatement());
            return ret;
        }

        private Statement HandleFigureDeclaration()
        {
            if(Accept(TokenType.Figure))
            {
                TokenInfo id = currentToken;
                Expect(TokenType.Identifier);
                Expect(TokenType.CurlyBracketLeft);
                var declStatements = GetDeclStatementBlock();
                return new FigureDeclarationStatement(id.code, declStatements);
            }
            return null;
        }

        private Statement HandleAnimationDeclaration()
        {
            if(Accept(TokenType.Animation))
            {
                TokenInfo id = currentToken;
                Expect(TokenType.Identifier);
                Queue<ArgumentDeclaration> args = GetArgs();
                Expect(TokenType.CurlyBracketLeft);
                Queue<Statement> blockStatements = GetStatementBlockInCurlyBracket();
                return new AnimationDeclarationStatement(id.code, args, blockStatements);
            }
            return null;
        }

        private Queue<ArgumentDeclaration> GetArgs()
        {
            Expect(TokenType.ParenthesisLeft);
            Queue<ArgumentDeclaration> args = new Queue<ArgumentDeclaration>();
            while(!Accept(TokenType.ParenthesisRight))
            {
                if(args.Count!=0)
                    Expect(TokenType.Comma, "Bad animation declaration in line " + currentToken.line);
                TokenInfo type = currentToken;
                Expect(TokenType.Identifier);
                TokenInfo name = currentToken;
                Expect(TokenType.Identifier);
                args.Enqueue(new ArgumentDeclaration(type.code, name.code));
            }
            return args;
        }

        private Statement HandleIfStatement()
        {
            if (Accept(TokenType.If))
            {
                Expect(TokenType.ParenthesisLeft);
                var expr1 = GetArithmExpr();
                var relationToken = currentToken;
                ExpectRelationOp();
                var relationOp = new RelationEvalutator(relationToken.code);
                var expr2 = GetArithmExpr();
                Expect(TokenType.ParenthesisRight);
                var blockStatements = GetStatementBlock();
                var elseBlockStatements = new Queue<Statement>();
                if (Accept(TokenType.Else))
                    elseBlockStatements = GetStatementBlock();
                return new IfStatement(expr1, relationOp, expr2, blockStatements, elseBlockStatements);
            }
            return null;
        }

        private Statement HandleForEachStatement()
        {
            if (Accept(TokenType.For))
            {
                Expect(TokenType.Each);
                TokenInfo idv = currentToken;
                Expect(TokenType.Identifier);
                Expect(TokenType.In);
                TokenInfo idc = currentToken;
                Expect(TokenType.Identifier);
                var idcc = new List<Identifier>();
                idcc.Add(new Identifier(idc.code,0));
                idcc = GetAttribToken(idcc);
                Queue<Statement> blockStatements = GetStatementBlock();
                return new ForEachStatement(idv.code, new ValueEvaluator(idcc), blockStatements);
            }
            return null;
        }

        private Statement HandleEachStatement()
        {
            if (Accept(TokenType.Each))
            {
                TokenInfo period = currentToken;
                Expect(TokenType.Integer);
                Queue<Statement> blockStatements = GetStatementBlock();
                return new EachStatement(period.value ?? default(int), blockStatements);
            }
            return null;
        }

        private Statement HandleVarAndCollDeclaration(TokenInfo type)
        {
            TokenInfo name = currentToken;
            if(Accept(TokenType.Identifier))
            {
                int index = HandleSubscriptOp();
                Expect(TokenType.Semicolon, "Expected semicolon at the end of line " + currentToken.line);
                if(index==-1)
                    return new VariableInstantiationStatement(type.code, name.code);
                else if(index==0)
                    throw new SyntaxException("Cannot declare collection of count 0 in line " + currentToken.line);
                else if(index>0)
                    return new CollectionInstantiationStatement(type.code, name.code, index);
                else
                    throw new SyntaxException("Cannot declare collection of negative count in line " + currentToken.line);
            }
            return null;
        }
        
        private Statement HandleAnimationCall(ValueEvaluator ids)
        {
            if(Accept(TokenType.ParenthesisLeft))
            {
                Queue<ArithmExprEvaluator> args = new Queue<ArithmExprEvaluator>();
                if(Accept(TokenType.ParenthesisRight))
                    return new AnimationCallStatement(ids, args);
                while(!Accept(TokenType.ParenthesisRight))
                {
                    if(args.Count!=0)
                        NextToken();
                    var arg = GetArithmExpr();
                    args.Enqueue(arg);
                    if(currentToken.token!=TokenType.ParenthesisRight && currentToken.token!=TokenType.Comma)
                        throw new SyntaxException("Bad animation call in line " + currentToken.line);
                }
                Expect(TokenType.Semicolon, "Expected semicolon at the end of line " + currentToken.line);
                return new AnimationCallStatement(ids, args);
            }
            return null;
        }

        private Statement HandleAssignment(ValueEvaluator lvalue)
        {

            if(Accept(TokenType.AssignmentOperator))
            {
                var expr = GetArithmExpr();
                Expect(TokenType.Semicolon, "Expected semicolon at the end of line " + currentToken.line);
                return new AssignmentStatement(lvalue, expr);
            }
            return null;
        }

        private int HandleSubscriptOp()
        {
            int ret = -1;
            if(Accept(TokenType.SquareBracketLeft))
            {
                ret = currentToken.value ?? default(int);
                Expect(TokenType.Integer);
                Expect(TokenType.SquareBracketRight);
            }
            return ret;
        }

        private ValueEvaluator AcceptValueToken()
        {
            var token = currentToken;
            if(Accept(TokenType.Integer))
                return new ValueEvaluator(token.value ?? default(int));
            var ids = new List<Identifier>();
            ids.Add(new Identifier(token.code,0));
            if(Accept(TokenType.Color))
                return new ValueEvaluator(ids);
            if(Accept(TokenType.Identifier))
                return new ValueEvaluator(GetAttribToken(ids));
            return null;
        }

        private List<Identifier> GetAttribToken(List<Identifier> ids)
        {
            int index = HandleSubscriptOp();
            if(index>=0)
            {
                Identifier id = ids[ids.Count-1];
                id.index = index;
                ids[ids.Count-1] = id;
            }
            ids = BuildAttribExpr(ids);
            return ids;
        }

        private List<Identifier> BuildAttribExpr(List<Identifier> ids)
        {
            while(Accept(TokenType.Dot))
            {
                TokenInfo token = currentToken;
                Expect(TokenType.Identifier);
                int index = HandleSubscriptOp();
                if(index>=0)
                    token.value = index;
                
                ids.Add(new Identifier(token.code, token.value ?? default(int)));
            }
            return ids;
        }

        private ArithmExprEvaluator GetArithmExpr()
        {
            var ret = new ArithmExprEvaluator();
            ret.expr = GetAdditionSubtractionExpr();
            return ret;
        }

        private AdditionSubtractionEvaluator GetAdditionSubtractionExpr()
        {
            var leftVal = GetMultiplicationDivisionExpr();
            var op = currentToken.code;
            if(Accept(TokenType.PlusOperator) || Accept(TokenType.MinusOperator))
                return GetAdditionSubtractionExprRight(leftVal, op);
            else
                return new AdditionSubtractionEvaluator(leftVal);
        }

        private AdditionSubtractionEvaluator GetAdditionSubtractionExprRight(MultiplicationDivisionEvaluator left, string oper)
        {
            var leftVal = left;
            var rightVal = GetMultiplicationDivisionExpr();
            var op = currentToken.code;
            if(Accept(TokenType.PlusOperator) || Accept(TokenType.MinusOperator))
            {
                leftVal = new MultiplicationDivisionEvaluator(new ArithmValueEvaluator(new ArithmExprEvaluator(new AdditionSubtractionEvaluator(leftVal, rightVal, oper))));
                return GetAdditionSubtractionExprRight(leftVal, op);
            }
            return new AdditionSubtractionEvaluator(leftVal, rightVal, oper);
        }

        private MultiplicationDivisionEvaluator GetMultiplicationDivisionExpr()
        {
            var leftVal = GetArithmValue();
            var op = currentToken.code;
            if(Accept(TokenType.AsteriskOperator) || Accept(TokenType.SlashOperator))
                return GetMultiplicationDivisionExprRight(leftVal, op);
            else
                return new MultiplicationDivisionEvaluator(leftVal);
        }

        private MultiplicationDivisionEvaluator GetMultiplicationDivisionExprRight(ArithmValueEvaluator left, string oper)
        {
            var leftVal = left;
            var rightVal = GetArithmValue();
            var op = currentToken.code;
            if(Accept(TokenType.AsteriskOperator) || Accept(TokenType.SlashOperator))
            {
                leftVal = new ArithmValueEvaluator(new ArithmExprEvaluator(new MultiplicationDivisionEvaluator(leftVal, rightVal, oper)));
                return GetMultiplicationDivisionExprRight(leftVal, op);
            }
            return new MultiplicationDivisionEvaluator(leftVal, rightVal, oper);
        }

        private ArithmValueEvaluator GetArithmValue()
        {
            var value = AcceptValueToken();
            if(value != null)
                return new ArithmValueEvaluator(value);
            var expr = AcceptNestedArithmExpr();
            if(expr != null)
                return new ArithmValueEvaluator(expr);
            throw new SyntaxException("Unexpected symbol in arithmetic expression in line " + currentToken.line);
        }

        private ArithmExprEvaluator AcceptNestedArithmExpr()
        {
            if(Accept(TokenType.ParenthesisLeft))
            {
                ArithmExprEvaluator expr = GetArithmExpr();
                Expect(TokenType.ParenthesisRight);
                return expr;
            }
            return null;
        }

        private bool ExpectRelationOp()
        {
            if (Accept(TokenType.EqualityOperator) || Accept(TokenType.InequalityOperator) || Accept(TokenType.GreaterOperator) || Accept(TokenType.GreaterOrEqualOperator) || Accept(TokenType.LessOrEqualOperator) || Accept(TokenType.LessOperator))
                return true;
            throw new SyntaxException("Expected relation operator in line " + currentToken.line);
        }

        private Queue<Statement> statements;
        private Lexer lexer;
        private TokenInfo currentToken;
    }

    class SyntaxException : Exception
    {
        public SyntaxException() {}
        public SyntaxException(string message) : base(message) { Console.WriteLine(message); }
        public SyntaxException(string message, Exception inner) : base(message,inner) { Console.WriteLine(message); }
    }
}
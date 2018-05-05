using System;
using System.Collections.Generic;

namespace Code
{
    class Parser
    {
        public Parser(string path)
        {
            lexer = new Lexer(path);
            statements = new List<Statement>();
            currentToken = lexer.NextToken();
        }

        public List<Statement> Run()
        {
            while(currentToken.token!=TokenType.EOF)
            {
                try
                {
                    statements.Add(NextStatement());
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
            List<TokenInfo> ids = new List<TokenInfo>();
            ids.Add(currentToken);
            Expect(TokenType.Identifier, "Unexpected symbol in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(ids[0]);
            if (ret!=null)
                return ret;
            ids = GetAttribToken(ids);
            ret = HandleAnimationCall(ids);
            if (ret!=null)
                return ret;
            ret = HandleAssignment(ids);
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
            List<TokenInfo> ids = new List<TokenInfo>();
            ids.Add(currentToken);
            Expect(TokenType.Identifier, "Unexpected symbol in declaration in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(ids[0]);
            if (ret!=null)
                return ret;
            ids = GetAttribToken(ids);
            ret = HandleAssignment(ids);
            if (ret!=null)
                return ret;
            if(Accept(TokenType.EOF))
                throw new SyntaxException("Unexpected end of file");
            throw new SyntaxException("Unexpected symbol in declaration in line: " + currentToken.line);
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
            List<TokenInfo> ids = new List<TokenInfo>();
            ids.Add(currentToken);
            Expect(TokenType.Identifier, "Unexpected symbol in line " + currentToken.line);
            ret = HandleVarAndCollDeclaration(ids[0]);
            if (ret!=null)
                return ret;
            ids = GetAttribToken(ids);
            ret = HandleAnimationCall(ids);
            if (ret!=null)
                return ret;
            ret = HandleAssignment(ids);
            if (ret!=null)
                return ret;
            if(Accept(TokenType.EOF))
                throw new SyntaxException("Unexpected end of file");
            throw new SyntaxException("Unexpected symbol in line: " + currentToken.line);
        }

        private Statement HandleFigureDeclaration()
        {
            if(Accept(TokenType.Figure))
            {
                TokenInfo id = currentToken;
                Expect(TokenType.Identifier);
                Expect(TokenType.CurlyBracketLeft);
                List<Statement> declStatements = new List<Statement>();
                while(!Accept(TokenType.CurlyBracketRight))
                    declStatements.Add(NextDeclStatement());
                return new FigureDeclarationStatement(id, declStatements);
            }
            return null;
        }

        private Statement HandleAnimationDeclaration()
        {
            if(Accept(TokenType.Animation))
            {
                TokenInfo id = currentToken;
                Expect(TokenType.Identifier);
                Queue<Tuple<TokenInfo, TokenInfo>> args = GetArgs();
                Expect(TokenType.CurlyBracketLeft);
                List<Statement> blockStatements = new List<Statement>();
                while(!Accept(TokenType.CurlyBracketRight))
                    blockStatements.Add(NextBlockStatement());
                return new AnimationDeclarationStatement(id, args, blockStatements);
            }
            return null;
        }

        private Queue<Tuple<TokenInfo, TokenInfo>> GetArgs()
        {
            Expect(TokenType.ParenthesisLeft);
            Queue<Tuple<TokenInfo, TokenInfo>> args = new Queue<Tuple<TokenInfo, TokenInfo>>();
            if(Accept(TokenType.ParenthesisRight))
                return args;
            while(true)
            {
                TokenInfo type = currentToken;
                Expect(TokenType.Identifier);
                TokenInfo name = currentToken;
                Expect(TokenType.Identifier);
                args.Enqueue(new Tuple<TokenInfo, TokenInfo>(type, name));
                if(Accept(TokenType.ParenthesisRight))
                    return args;
                else if(args.Count!=0 && currentToken.token!=TokenType.Comma)
                    throw new SyntaxException("Bad animation declaration in line " + currentToken.line);
                NextToken();
            }
        }

        private Statement HandleIfStatement()
        {
            if (Accept(TokenType.If))
            {
                var token = new List<TokenInfo>();
                var lastToken = new List<TokenInfo>();
                token.Add(new TokenInfo(TokenType.AssignmentOperator, "=", 0, currentToken.line));
                Expect(TokenType.ParenthesisLeft);
                var expr1 = GetArithmExpr(lastToken, token);
                TokenInfo relationOp = currentToken;
                ExpectRelationOp();
                var expr2 = GetArithmExpr(lastToken, token);
                Expect(TokenType.ParenthesisRight);
                List<Statement> blockStatements = new List<Statement>();
                List<Statement> elseBlockStatements = new List<Statement>();
                if(Accept(TokenType.CurlyBracketLeft))
                {
                    while(!Accept(TokenType.CurlyBracketRight))
                    blockStatements.Add(NextBlockStatement());
                }
                blockStatements.Add(NextBlockStatement());
                if (Accept(TokenType.Else))
                {  
                    if(Accept(TokenType.CurlyBracketLeft))
                    {
                        while(!Accept(TokenType.CurlyBracketRight))
                        elseBlockStatements.Add(NextBlockStatement());
                    }
                    elseBlockStatements.Add(NextBlockStatement());
                }
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
                List<Statement> blockStatements = new List<Statement>();
                if(Accept(TokenType.CurlyBracketLeft))
                {
                    while(!Accept(TokenType.CurlyBracketRight))
                    blockStatements.Add(NextBlockStatement());
                }
                blockStatements.Add(NextBlockStatement());
                return new ForEachStatement(idv, idc, blockStatements);
            }
            return null;
        }

        private Statement HandleEachStatement()
        {
            if (Accept(TokenType.Each))
            {
                TokenInfo period = currentToken;
                Expect(TokenType.Integer);
                List<Statement> blockStatements = new List<Statement>();
                if(Accept(TokenType.CurlyBracketLeft))
                {
                    while(!Accept(TokenType.CurlyBracketRight))
                    blockStatements.Add(NextBlockStatement());
                }
                blockStatements.Add(NextBlockStatement());
                return new EachStatement(period, blockStatements);
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
                    return new VariableInstantiationStatement(type, name);
                else if(index==0)
                    throw new SyntaxException("Cannot declare collection of count 0 in line " + currentToken.line);
                else
                    return new CollectionInstantiationStatement(type, name, index);
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

        private Statement HandleAnimationCall(List<TokenInfo> ids)
        {
            if(Accept(TokenType.ParenthesisLeft))
            {
                Queue<List<TokenInfo>> args = new Queue<List<TokenInfo>>();
                if(Accept(TokenType.ParenthesisRight))
                    return new AnimationCallStatement(ids, args);
                while(true)
                {
                    List<TokenInfo> valueToken =  GetValueToken();
                    if (valueToken!=null)
                        args.Enqueue(valueToken);
                    else
                        throw new SyntaxException("Bad animation call in line " + currentToken.line);
                    if(Accept(TokenType.ParenthesisRight))
                    {
                        Expect(TokenType.Semicolon, "Expected semicolon at the end of line " + currentToken.line);
                        return new AnimationCallStatement(ids, args);
                    }
                    if(args.Count!=0 && currentToken.token!=TokenType.Comma)
                        throw new SyntaxException("Bad animation call in line " + currentToken.line);
                    NextToken();
                }
            }
            return null;
        }

        private List<TokenInfo> GetValueToken()
        {
            List<TokenInfo> ids = new List<TokenInfo>();
            TokenInfo token = currentToken;
            ids.Add(token);
            if(Accept(TokenType.Integer) || Accept(TokenType.Color))
                return ids;
            Expect(TokenType.Identifier, "Expected literal or variable with value in line " + currentToken.line);
            return GetAttribToken(ids);
        }

        private List<TokenInfo> AcceptArithmToken(int deepness)
        {
            List<TokenInfo> ids = new List<TokenInfo>();
            TokenInfo token = currentToken;
            ids.Add(token);
            if(Accept(TokenType.Integer) || AcceptArithmOp() || Accept(TokenType.ParenthesisLeft) || (deepness>0 && Accept(TokenType.ParenthesisRight)))
                return ids;
            if(Accept(TokenType.Identifier))
                return GetAttribToken(ids);
            return null;
        }

        private List<TokenInfo> AcceptValueToken()
        {
            List<TokenInfo> ids = new List<TokenInfo>();
            TokenInfo token = currentToken;
            ids.Add(token);
            if(Accept(TokenType.Integer) || Accept(TokenType.Color))
                return ids;
            if(Accept(TokenType.Identifier))
                return GetAttribToken(ids);
            return null;
        }

        private List<TokenInfo> GetAttribToken(List<TokenInfo> ids)
        {
            int index = HandleSubscriptOp();
            if(index>=0)
            {
                TokenInfo token = ids[ids.Count-1];
                token.value = index;
                ids[ids.Count-1] = token;
            }
            ids = BuildAttribExpr(ids);
            return ids;
        }

        private List<TokenInfo> BuildAttribExpr(List<TokenInfo> ids)
        {
            while(Accept(TokenType.Dot))
            {
                TokenInfo token = currentToken;
                Expect(TokenType.Identifier);
                int index = HandleSubscriptOp();
                if(index>=0)
                    token.value = index;
                ids.Add(token);
            }
            return ids;
        }

        private Statement HandleAssignment(List<TokenInfo> lvalue)
        {
            var token = new List<TokenInfo>();
            var lastToken = new List<TokenInfo>();
            token.Add(currentToken);
            if(Accept(TokenType.AssignmentOperator))
            {
                var queue = new Queue<List<TokenInfo>>();
                if (Accept(TokenType.Color))
                {
                    var tmp = new List<TokenInfo>();
                    tmp.Add(currentToken);
                    queue.Enqueue(tmp);
                }
                else
                    queue = GetArithmExpr(lastToken, token);
                Expect(TokenType.Semicolon, "Expected semicolon at the end of line " + currentToken.line);
                return new AssignmentStatement(lvalue, queue);
            }
            return null;
        }

        private Queue<List<TokenInfo>> GetArithmExpr(List<TokenInfo> lastToken, List<TokenInfo> token)
        {
            var outputQueue = new Queue<List<TokenInfo>>();
            var stack = new Stack<List<TokenInfo>>();
            int deepness = 0;
            while(true)
            {
                lastToken = token;
                token = AcceptArithmToken(deepness);
                if (token==null)
                    break;
                if(!IsTokenValid(lastToken[0], token[0], deepness))
                    throw new SyntaxException("Meaningless arithmetical expression in line " + currentToken.line);
                var tmp = updateQueue(outputQueue, stack, token, deepness);
                outputQueue = tmp.Item1;
                stack = tmp.Item2;
                deepness = tmp.Item3;
            }
            while(stack.Count>0)
            {
                outputQueue.Enqueue(stack.Pop());
            }
            return outputQueue;
        }

        private Tuple<Queue<List<TokenInfo>>, Stack<List<TokenInfo>>, int> updateQueue(Queue<List<TokenInfo>> queue, Stack<List<TokenInfo>> stack, List<TokenInfo> token, int deepness)
        {
            if (token[0].token==TokenType.Identifier || token[0].token==TokenType.Integer)
                queue.Enqueue(token);
            else if (token[0].token==TokenType.ParenthesisLeft)
            {
                stack.Push(token);
                deepness++;
            }
            else if (token[0].token==TokenType.ParenthesisRight)
            {
                var popStack = stack.Pop();
                while (popStack[0].token!=TokenType.ParenthesisLeft)
                {
                    queue.Enqueue(popStack);
                    popStack = stack.Pop();
                }
                deepness--;
            }
            else if (IsArithmOp(token[0]))
            {
                if((token[0].token==TokenType.PlusOperator || token[0].token==TokenType.MinusOperator) && stack.Count!=0)
                {
                    var tmp = stack.Pop();
                    while(tmp[0].token==TokenType.AsteriskOperator || tmp[0].token==TokenType.SlashOperator)
                    {
                        queue.Enqueue(tmp);
                        tmp = stack.Pop();
                    }
                    stack.Push(tmp);
                }
                stack.Push(token);
            }
            return new Tuple<Queue<List<TokenInfo>>, Stack<List<TokenInfo>>, int>(queue, stack, deepness);
        }

        private bool IsTokenValid(TokenInfo lastToken, TokenInfo token,int deepness)
        {
            if((lastToken.token==TokenType.AssignmentOperator || IsArithmOp(lastToken) || lastToken.token==TokenType.ParenthesisLeft) && token.token==TokenType.ParenthesisLeft)
                return true;
            else if((lastToken.token==TokenType.Identifier || lastToken.token==TokenType.Integer) && deepness>0 && token.token==TokenType.ParenthesisRight)
                return true;
            else if((lastToken.token==TokenType.Identifier || lastToken.token==TokenType.Integer || lastToken.token==TokenType.ParenthesisRight) && IsArithmOp(token))
                return true;
            else if((lastToken.token==TokenType.AssignmentOperator || IsArithmOp(lastToken) || lastToken.token==TokenType.ParenthesisLeft) && (token.token==TokenType.Integer || token.token==TokenType.Identifier))
                return true;
            return false;
        }

        private bool IsArithmOp(TokenInfo token)
        {
            return (token.token==TokenType.PlusOperator || token.token==TokenType.MinusOperator || token.token==TokenType.AsteriskOperator || token.token==TokenType.SlashOperator);
        }

        private bool AcceptArithmOp()
        {
            return (Accept(TokenType.PlusOperator) || Accept(TokenType.MinusOperator) || Accept(TokenType.AsteriskOperator) || Accept(TokenType.SlashOperator));
        }

        private bool ExpectRelationOp()
        {
            if (Accept(TokenType.EqualityOperator) || Accept(TokenType.InequalityOperator) || Accept(TokenType.GreaterOperator) || Accept(TokenType.GreaterOrEqualOperator) || Accept(TokenType.LessOrEqualOperator) || Accept(TokenType.LessOperator))
                return true;
            throw new SyntaxException("Expected relation operator in line " + currentToken.line);
        }

        private List<Statement> statements;
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
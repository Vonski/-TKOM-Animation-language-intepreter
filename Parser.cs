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
            ret = null; // if
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
            // switch(currentToken.token)
            // {
            //     case TokenType.Figure: return HandleFigureDeclaration();
            //     // case TokenType.Animation: return HandleAnimationDeclaration();
            //     // case TokenType.If: /* funcall */ break;
            //     // case TokenType.For: return HandleForEachStatement();
            //     // case TokenType.Each: return HandleEachStatement();
            //     // case TokenType.Identifier:
            //     //     string id = currentToken.code;
            //     //     NextToken();
            //     //     if(currentToken.token==TokenType.Identifier)
            //     //         return HandleInstantiation(id);
            //     //     else if(currentToken.token==TokenType.Dot || currentToken.token==TokenType.AssignmentOperator)
            //     //         return HandleAttribAssignment(id);
            //     //     else
            //     //         return HandleAnimationCall(id);
            //     default:
            //         throw new SyntaxException("Unexpected symbol in line: " + currentToken.line);
            // }
            throw new SyntaxException("Unexpected symbol in line: " + currentToken.line);    
            //return null;
        }

        private Statement HandleFigureDeclaration()
        {
            if(Accept(TokenType.Figure))
            {
                TokenInfo id = currentToken;
                Expect(TokenType.Identifier);
                /* Figure declaration block funcall */
                return new FigureDeclarationStatement(id, new List<Statement>());
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
                /* Block funcall */
                return new AnimationDeclarationStatement(id, args, new List<Statement>());
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
                /* Block funcall */
                return new ForEachStatement(idv, idc, new List<Statement>());
            }
            return null;
        }

        private Statement HandleEachStatement()
        {
            if (Accept(TokenType.Each))
            {
                TokenInfo period = currentToken;
                Expect(TokenType.Integer);
                /* Block funcall */
                return new EachStatement(period, new List<Statement>());
            }
            return null;
        }

        private Statement HandleVarAndCollDeclaration(TokenInfo type)
        {
            TokenInfo name = currentToken;
            if(Accept(TokenType.Identifier))
            {
                int index = HandleSubscriptOp();
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
                        return new AnimationCallStatement(ids, args);
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

        private List<TokenInfo> AcceptArithmToken()
        {
            List<TokenInfo> ids = new List<TokenInfo>();
            TokenInfo token = currentToken;
            ids.Add(token);
            if(Accept(TokenType.Integer) || AcceptArithmOp() || Accept(TokenType.ParenthesisLeft) || Accept(TokenType.ParenthesisRight))
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
                var queue = GetArithmExpr(lastToken, token);
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
                token = AcceptArithmToken();
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
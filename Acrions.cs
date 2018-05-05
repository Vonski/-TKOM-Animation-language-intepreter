using System;
using System.Collections.Generic;

namespace Code
{
    enum StatementType
    {
        AnimationCall,
        VariableDeclaration,
        CollectionDeclaration,
        Each,
        ForEach,
        AnimationDeclaration,
        FigureDeclaration,
        Assignment,
        If,
        Other
    }
    abstract class Statement
    {
        public StatementType type;
    }

    class FigureDeclarationStatement : Statement
    {
        public FigureDeclarationStatement(TokenInfo idd, List<Statement> list)
        {
            type = StatementType.FigureDeclaration;
            id = idd;
            statements = list;
        }

        public override string ToString()
        {
            return type + ", " + id.code;
        }
        
        TokenInfo id;
        List<Statement> statements;
    }

    class AnimationDeclarationStatement : Statement
    {
        public AnimationDeclarationStatement(TokenInfo idd, Queue<Tuple<TokenInfo, TokenInfo>> arguments, List<Statement> list)
        {
            type = StatementType.AnimationDeclaration;
            id = idd;
            args = arguments;
            statements = list;
        }

        public override string ToString()
        {
            string ret = "(";
            foreach(Tuple<TokenInfo, TokenInfo> arg in args)
                ret +=  arg.Item1.code + " " + arg.Item2.code + ", ";
            ret = ret.Remove(ret.Length-2);
            ret += ")";
            return type + ", " + id.code + ret;
        }

        TokenInfo id;
        Queue<Tuple<TokenInfo, TokenInfo>> args;
        List<Statement> statements;
    }

    class IfStatement : Statement
    {
        public IfStatement(Queue<List<TokenInfo>> e1, TokenInfo relOp, Queue<List<TokenInfo>> e2, List<Statement> stat, List<Statement> elseStat)
        {
            type = StatementType.If;
            expr1 = e1;
            relationOp = relOp;
            expr2 = e2;
            statements = stat;
            elseStatements = elseStat;
        }

        public override string ToString()
        {
            string ret = "";
            foreach(var token in expr1)
                if(token[0].value!=null)
                    ret += " " + token[0].value;
                else
                    ret += " " + token[0].code;
            ret += ", " + relationOp.code + ", ";
            foreach(var token in expr2)
                if(token[0].value!=null)
                    ret += " " + token[0].value;
                else
                    ret += " " + token[0].code;
            return type + ", " + ret;
        }

        TokenInfo relationOp;
        Queue<List<TokenInfo>> expr1, expr2;
        List<Statement> statements, elseStatements;
    }

    class ForEachStatement : Statement
    {
        public ForEachStatement(TokenInfo idv, TokenInfo idc, List<Statement> list)
        {
            type = StatementType.ForEach;
            iterator_id = idv;
            collection_id = idc;
            statements = list;
        }

        public override string ToString()
        {
            return type + ", " + iterator_id.code + " in " + collection_id.code;
        }

        TokenInfo iterator_id, collection_id;
        List<Statement> statements;
    }

    class EachStatement : Statement
    {
        public EachStatement(TokenInfo p, List<Statement> list)
        {
            type = StatementType.Each;
            period = p;
            statements = list;
        }

        public override string ToString()
        {
            return type + ", " + period.value;
        }

        TokenInfo period;
        List<Statement> statements;
    }

    class VariableInstantiationStatement : Statement
    {
        public VariableInstantiationStatement(TokenInfo t, TokenInfo n)
        {
            type = StatementType.VariableDeclaration;
            varType = t;
            name = n;
        }

        public override string ToString()
        {
            return type + ", " + varType.code + " " + name.code;
        }

        TokenInfo varType, name;
    }

    class CollectionInstantiationStatement : Statement
    {
        public CollectionInstantiationStatement(TokenInfo t, TokenInfo n, int c)
        {
            type = StatementType.CollectionDeclaration;
            varType = t;
            name = n;
            count = c;
        }

        public override string ToString()
        {
            return type + ", " + varType.code + " " + name.code + "[" + count + "]";
        }

        TokenInfo varType, name;
        int count;
    }

    class AssignmentStatement : Statement
    {
        public AssignmentStatement(List<TokenInfo> idd, Queue<List<TokenInfo>> arguments)
        {
            type = StatementType.Assignment;
            ids = idd;
            arithmeticExpression = arguments;
        }

        public override string ToString()
        {
            string ret = ids[0].code + ", ";
            foreach(var token in arithmeticExpression)
                if(token[0].value!=null)
                    ret += " " + token[0].value;
                else
                    ret += " " + token[0].code;
            return type.ToString() + ", " + ret;
        }

        List<TokenInfo> ids;
        Queue<List<TokenInfo>> arithmeticExpression;
    }

    class AnimationCallStatement : Statement
    {
        public AnimationCallStatement(List<TokenInfo> idd, Queue<List<TokenInfo>> queue)
        {
            type = StatementType.AnimationCall;
            ids = idd;
            args = queue;
        }

        public override string ToString()
        {
            string ret = "";
            foreach(TokenInfo token in ids)
                if(token.value!=null)
                    ret += "." + token.value;
                else
                    ret += "." + token.code;
            ret = ret.Substring(1) + "(";
            foreach(List<TokenInfo> list in args)
            {
                string tmp = "";
                foreach(TokenInfo token in list)
                    if(token.value!=null)
                        tmp += "." + token.value;
                    else
                        tmp += "." + token.code;
                ret += tmp.Substring(1) + ", ";
            }
            ret = ret.Remove(ret.Length-2);
            ret += ")";
            return type + ", " + ret;
        }

        List<TokenInfo> ids;
        Queue<List<TokenInfo>> args;
    }
}
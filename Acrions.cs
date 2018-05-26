using System;
using System.Collections.Generic;

namespace Code
{
    interface Statement
    {
        void execute();
    }

    class FigureDeclarationStatement : Statement
    {
        public FigureDeclarationStatement(string idd, List<Statement> list)
        {
            id = idd;
            statements = list;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "FigureDeclaration: " + id;
        }

        string id;
        List<Statement> statements;
    }

    class AnimationDeclarationStatement : Statement
    {
        public AnimationDeclarationStatement(string idd, Queue<ArgumentDeclaration> arguments, List<Statement> list)
        {
            id = idd;
            args = arguments;
            statements = list;
        }
        
        public void execute() {}

        public override string ToString()
        {
            string ret = "(";
            foreach(ArgumentDeclaration arg in args)
                ret +=  arg.type + " " + arg.name + ", ";
            ret = ret.Remove(ret.Length-2);
            ret += ")";
            return "AnimationDeclaration: " + id + ret;
        }

        string id;
        Queue<ArgumentDeclaration> args;
        List<Statement> statements;
    }

    class IfStatement : Statement
    {
        public IfStatement(ArithmExprEvaluator e1, RelationEvalutator relOp, ArithmExprEvaluator e2, List<Statement> stat, List<Statement> elseStat)
        {
            expr1 = e1;
            relationOp = relOp;
            expr2 = e2;
            statements = stat;
            elseStatements = elseStat;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "If: " + expr1.ToString() + ", " + relationOp.relationOp + ", " + expr2.ToString();
        }

        RelationEvalutator relationOp;
        ArithmExprEvaluator expr1, expr2;
        List<Statement> statements, elseStatements;
    }

    class ForEachStatement : Statement
    {
        public ForEachStatement(string idv, ValueEvaluator idc, List<Statement> list)
        {
            iterator_id = idv;
            collection_id = idc;
            statements = list;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "ForEach: " + iterator_id + " in " + collection_id.ToString();
        }

        string iterator_id;
        ValueEvaluator collection_id;
        List<Statement> statements;
    }

    class EachStatement : Statement
    {
        public EachStatement(int p, List<Statement> list)
        {
            period = p;
            statements = list;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "Each: " + period;
        }

        int period;
        List<Statement> statements;
    }

    class VariableInstantiationStatement : Statement
    {
        public VariableInstantiationStatement(string t, string n)
        {
            varType = t;
            name = n;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "VariableDeclaration: " + varType + " " + name;
        }

        string varType, name;
    }

    class CollectionInstantiationStatement : Statement
    {
        public CollectionInstantiationStatement(string t, string n, int c)
        {
            varType = t;
            name = n;
            count = c;
        }
        
        public void execute() {}

        public override string ToString()
        {
            return "CollectionDeclaration: " + varType + " " + name + "[" + count + "]";
        }

        string varType, name;
        int count;
    }

    class AnimationCallStatement : Statement
    {
        public AnimationCallStatement(ValueEvaluator idd, Queue<ArithmExprEvaluator> queue)
        {
            ids = idd;
            args = queue;
        }
        
        public void execute() {}

        public override string ToString()
        {
            string ret = "";
            ret = ids.ToString() + "(";
            foreach(var arg in args)
                ret += arg.ToString() + ",";
            ret += ret.Remove(ret.Length-1) + ")";
            return "AnimationCall: " + ret;
        }

        ValueEvaluator ids;
        Queue<ArithmExprEvaluator> args;
    }

    class AssignmentStatement : Statement
    {
        public AssignmentStatement(ValueEvaluator idd, ArithmExprEvaluator arguments)
        {
            ids = idd;
            arithmeticExpression = arguments;
        }
        
        public void execute() {}

        public override string ToString()
        {
            string ret = ids.ToString() + "=" +  arithmeticExpression.ToString();
            return "Assignment: " + ret;
        }

        ValueEvaluator ids;
        ArithmExprEvaluator arithmeticExpression;
    }
}
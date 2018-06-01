using System;
using System.Collections.Generic;

namespace Code
{
    interface Statement
    {
        void execute(ScopesManager sm);
    }

    class FigureDeclarationStatement : Statement
    {
        public FigureDeclarationStatement(string idd, Queue<Statement> list)
        {
            id = idd;
            statements = list;
        }
        
        public void execute(ScopesManager sm) {}

        public override string ToString()
        {
            return "FigureDeclaration: " + id;
        }

        string id;
        Queue<Statement> statements;
    }

    class AnimationDeclarationStatement : Statement
    {
        public AnimationDeclarationStatement(string idd, Queue<ArgumentDeclaration> arguments, Queue<Statement> list)
        {
            id = idd;
            args = arguments;
            statements = list;
        }
        
        public void execute(ScopesManager sm) {}

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
        Queue<Statement> statements;
    }

    class IfStatement : Statement
    {
        public IfStatement(ArithmExprEvaluator e1, RelationEvalutator relOp, ArithmExprEvaluator e2, Queue<Statement> stat, Queue<Statement> elseStat)
        {
            expr1 = e1;
            relationOp = relOp;
            expr2 = e2;
            statements = stat;
            elseStatements = elseStat;
        }
        
        public void execute(ScopesManager sm)
        {

            sm.NewScope();
            Statement stat;
            if (relationOp.eval(expr1, expr2))
                while(statements.Count>0)
                {
                    stat = statements.Dequeue();
                    stat.execute(sm);
                }
            else
                while (statements.Count > 0)
                {
                    stat = statements.Dequeue();
                    stat.execute(sm);
                }

            //sm.DestroyScope();
        }

        public override string ToString()
        {
            return "If: " + expr1.ToString() + ", " + relationOp.relationOp + ", " + expr2.ToString();
        }

        RelationEvalutator relationOp;
        ArithmExprEvaluator expr1, expr2;
        Queue<Statement> statements, elseStatements;
    }

    class ForEachStatement : Statement
    {
        public ForEachStatement(string idv, ValueEvaluator idc, Queue<Statement> list)
        {
            iterator_id = idv;
            collection_id = idc;
            statements = list;
        }
        
        public void execute(ScopesManager sm) {}

        public override string ToString()
        {
            return "ForEach: " + iterator_id + " in " + collection_id.ToString();
        }

        string iterator_id;
        ValueEvaluator collection_id;
        Queue<Statement> statements;
    }

    class EachStatement : Statement
    {
        public EachStatement(int p, Queue<Statement> list)
        {
            period = p;
            statements = list;
        }
        
        public void execute(ScopesManager sm) {}

        public override string ToString()
        {
            return "Each: " + period;
        }

        int period;
        Queue<Statement> statements;
    }

    class VariableInstantiationStatement : Statement
    {
        public VariableInstantiationStatement(string t, string n)
        {
            varType = t;
            name = n;
        }
        
        public void execute(ScopesManager sm)
        {
            IComposite variable = sm.GetMyType(varType);
            if (variable != null)
            {
                var tmp = Prototype.CloneObject(variable) as IComposite;
                tmp.Name = name;
                sm.declareVar(name, tmp);
            }
            else
                throw new RuntimeException("Nie ma takiego typu");
        }

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
        
        public void execute(ScopesManager sm)
        {
            IComposite variable = sm.GetMyType(varType);
            Composite collection = new Composite(name);
            if (variable == null)
                throw new RuntimeException("Nie ma takiego typu");

            for (int i = 0; i < count; ++i)
            {
                IComposite tmp2 = Prototype.CloneObject(variable) as IComposite;
                tmp2.Position += new SFML.System.Vector2f(i*30.0f, i*30.0f);
                collection.Add(tmp2);
            }
            
            sm.declareVar(name, collection);
        }

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
        
        public void execute(ScopesManager sm) {}

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
        
        public void execute(ScopesManager sm)
        {

        }

        public override string ToString()
        {
            string ret = ids.ToString() + "=" +  arithmeticExpression.ToString();
            return "Assignment: " + ret;
        }

        ValueEvaluator ids;
        ArithmExprEvaluator arithmeticExpression;
    }

    class RuntimeException : Exception
    {
        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { Console.WriteLine(message); }
        public RuntimeException(string message, Exception inner) : base(message, inner) { Console.WriteLine(message); }
    }
}
using System;
using System.Collections.Generic;

namespace Code
{
    class ArgumentDeclaration
    {
        public ArgumentDeclaration(string ttype, string nname)
        {
            type=ttype;
            name=nname;
        }
        public string type;
        public string name;
    }

    class RelationEvalutator
    {
        public RelationEvalutator(string op)
        {
            relationOp = op;
        }
        public bool eval(ArithmExprEvaluator expr1, ArithmExprEvaluator expr2)
        {
            switch(relationOp)
            {
                case "==":
                    if (expr1.eval() == expr2.eval())
                        return true;
                    break;
                case "!=":
                    if (expr1.eval() != expr2.eval())
                        return true;
                    break;
                case ">=":
                    if (expr1.eval() >= expr2.eval())
                        return true;
                    break;
                case ">":
                    if (expr1.eval() > expr2.eval())
                        return true;
                    break;
                case "<=":
                    if (expr1.eval() <= expr2.eval())
                        return true;
                    break;
                case "<":
                    if (expr1.eval() < expr2.eval())
                        return true;
                    break;
                default:
                    throw new UnexpectedException("Bad relation operator! CRITICAL ERROR!!!");
            }
            return false;
        }
        public string relationOp;
    }

    class Identifier
    {
        public Identifier(string n, int c)
        {
            name = n;
            index = c;
        }
        public override string ToString()
        {
            return name + "[" + index + "]";
        }
        public string name;
        public int index;
    }

    class ValueEvaluator
    {
        public ValueEvaluator(int val)
        {
            value = val;
            ids = null;
        }

        public ValueEvaluator(List<Identifier> idss)
        {
            value = null;
            ids = idss;
        }

        public int eval()
        {
            if (value!=null)
                return value ?? default(int);   // dopisz wybieranie wartości z tablicy
            return 0;
        }

        public override string ToString()
        {
            var ret = "";
            if(value==null)
                foreach(var id in ids)
                    ret += id + ".";
            else
                ret += (value ?? default(int)) + ".";
            return ret.Remove(ret.Length-1);
        }

        public List<Identifier> ids;
        public int? value;
    }

    class ArithmValueEvaluator
    {
        public ArithmValueEvaluator(ValueEvaluator val)
        {
            simpleVal = val;
            arithmVal = null;
        }
        public ArithmValueEvaluator(ArithmExprEvaluator val)
        {
            simpleVal = null;
            arithmVal = val;
        }
        public int eval()
        {
            if (simpleVal!=null)
                return simpleVal.eval();
            else
                return arithmVal.eval();
        }
        public override string ToString()
        {
            if(simpleVal!=null)
                return simpleVal.ToString();
            else
                return arithmVal.ToString();
        }
        public ValueEvaluator simpleVal;
        public ArithmExprEvaluator arithmVal;
    }

    class MultiplicationDivisionEvaluator
    {
        public MultiplicationDivisionEvaluator(ArithmValueEvaluator l)
        {
            left = l;
            right = null;
            arithmOperator = null;
        }
        public MultiplicationDivisionEvaluator(ArithmValueEvaluator l, ArithmValueEvaluator r, string op)
        {
            left = l;
            right = r;
            arithmOperator = op;
        }
        public MultiplicationDivisionEvaluator(MultiplicationDivisionEvaluator l, ArithmValueEvaluator r, string op)
        {
            left = new ArithmValueEvaluator(new ArithmExprEvaluator(l));
            right = r;
            arithmOperator = op;
        }
        public int eval()
        {
            if (arithmOperator==null)
                return left.eval();
            else if (arithmOperator=="*")
                return left.eval() * right.eval();
            else if (arithmOperator=="/")
                return left.eval() / right.eval();
            throw new UnexpectedException("Bład mnozenia");
        }
        public override string ToString()
        {
            if(right!=null)
                return left.ToString() + arithmOperator + right.ToString();
            else
                return left.ToString();
        }
        public ArithmValueEvaluator left, right;
        public string arithmOperator;
    }

    class AdditionSubtractionEvaluator
    {
        public AdditionSubtractionEvaluator(MultiplicationDivisionEvaluator l)
        {
            left = l;
            right = null;
            arithmOperator = null;
        }
        public AdditionSubtractionEvaluator(MultiplicationDivisionEvaluator l, MultiplicationDivisionEvaluator r, string op)
        {
            left = l;
            right = r;
            arithmOperator = op;
        }
        public AdditionSubtractionEvaluator(AdditionSubtractionEvaluator l, MultiplicationDivisionEvaluator r, string op)
        {
            left = new MultiplicationDivisionEvaluator(new ArithmValueEvaluator(new ArithmExprEvaluator(l)));
            right = r;
            arithmOperator = op;
        }
        public int eval()
        {
            if (arithmOperator==null)
                return left.eval();
            else if (arithmOperator=="+")
                return left.eval() + right.eval();
            else if (arithmOperator=="-")
                return left.eval() - right.eval();
            throw new UnexpectedException("Bład mnozenia");
        }
        public override string ToString()
        {
            if(right!=null)
                return left.ToString() + arithmOperator + right.ToString();
            else
                return left.ToString();
        }
        public MultiplicationDivisionEvaluator left, right;
        public string arithmOperator;
    }

    class ArithmExprEvaluator
    {
        public ArithmExprEvaluator(MultiplicationDivisionEvaluator l) : this(new AdditionSubtractionEvaluator(l)) {}
        public ArithmExprEvaluator(AdditionSubtractionEvaluator l) { expr = l; }
        public ArithmExprEvaluator()
        {
        }
        public int eval()
        {
            return expr.eval();
        }
        public override string ToString()
        {
            return "(" + expr.ToString() + ")";
        }
        public AdditionSubtractionEvaluator expr;
    }

    class UnexpectedException : Exception
    {
        public UnexpectedException() {}
        public UnexpectedException(string message) : base(message) { Console.WriteLine(message); }
        public UnexpectedException(string message, Exception inner) : base(message,inner) { Console.WriteLine(message); }
    }
}
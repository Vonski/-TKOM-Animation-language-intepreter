using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.Graphics;
using SFML.Window;

namespace Code
{
    class Scope
    {
        public Scope()
        {
            varMap = new Dictionary<string, IComposite>();
        }
        public void Add(string varname, IComposite obj)
        {
            varMap[varname] = obj;
        }
        public IComposite GetVar(string varname)
        {
            IComposite tmp;
            if (varMap.TryGetValue(varname, out tmp))
                return tmp;
            else
                return null;
        }
        public void Update(float dT)
        {
            foreach (var pair in varMap)
                pair.Value.Update(dT);
        }
        public void Draw(RenderWindow app)
        {
            foreach (var pair in varMap)
                pair.Value.Draw(app);
        }

        Dictionary<string, IComposite> varMap;
    }

    class StaticScope
    {
        public StaticScope()
        {
            typeMap = new Dictionary<string, IComposite>();
        }
        public void Add(string typename, IComposite type)
        {
            typeMap[typename] = type;
        }
        public IComposite GetMyType(string typename)
        {
            IComposite tmp;
            if (typeMap.TryGetValue(typename, out tmp))
                return tmp;
            else
                return null;
        }
        Dictionary<string, IComposite> typeMap;
    }

    class ScopesManager
    {
        public ScopesManager()
        {
            scopesStack = new Stack<Scope>();
            scopesStack.Push(new Scope());
            typeScope = new StaticScope();
            Line defaultLine = new Line();
            Circle defaultCircle = new Code.Circle();
            typeScope.Add("line", defaultLine);
            typeScope.Add("circle", defaultCircle);
        }

        public void NewScope()
        {
            scopesStack.Push(new Scope());
        }
        public void DestroyScope()
        {
            scopesStack.Pop();
        }
        public void declareType(string typename, IComposite type)
        {
            typeScope.Add(typename, type);
        }
        public IComposite GetMyType(string typename)
        {
            return typeScope.GetMyType(typename);
        }

        public void declareVar(string varname, IComposite obj)
        {
            scopesStack.Peek().Add(varname, obj);
        }
        public IComposite GetVar(string varname)
        {
            int counter = scopesStack.Count;
            foreach(var scope in scopesStack)
            {
                counter--;

                var variable = scope.GetVar(varname);
                if (variable != null)
                {
                    Console.WriteLine("Variable: " + varname + " found in " + counter + ". scope");
                    return variable;
                }
            }
            return null;
        }
        public void Update(float dT)
        {
            foreach (var scope in scopesStack)
                scope.Update(dT);
        }
        public void Draw(RenderWindow app)
        {
            foreach (var scope in scopesStack)
                scope.Draw(app);
        }

        Stack<Scope> scopesStack;
        StaticScope typeScope;
    }

}

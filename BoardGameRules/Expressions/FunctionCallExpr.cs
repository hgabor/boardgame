using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class FunctionCallExpr: Expression
    {
        Expression name;
        Expression[] p;

        public FunctionCallExpr(Expression name, params Expression[] p)
        {
            this.name = name;
            this.p = (Expression[])p.Clone();
        }

        public override object Eval(Context c)
        {
            var f = (ICallable)name.Eval(c);
            if (f == null) throw new InvalidGameException("Invalid function: " + name);
            object[] args = new object[p.Length];
            //args[0] = c;
            for (int i = 0; i < p.Length; i++)
            {
                args[i] = p[i].Eval(c);
            }
            return f.Call(Context.NewLocal(c.Game), args);
        }

        static FunctionCallExpr()
        {
            Expression.RegisterParser("FUNCCALL", tree =>
            {
                Expression name = tree.GetChild("FUNCNAME").GetOnlyChild().ParseExpr();
                List<Expression> l = new List<Expression>();
                for (int i = 0; i < tree.GetChild("LIST").ChildCount; i++)
                {
                    l.Add(tree.GetChild("LIST").GetChild(i).ParseExpr());
                }
                return new FunctionCallExpr(name, l.ToArray());
            });
        }

        public override string ToString()
        {
            string args = string.Join(", ", (object[])p);
            return string.Format("{0}({1})", name, args);
        }
    }
}

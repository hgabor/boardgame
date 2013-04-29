using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
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
            Function f = (Function)name.Eval(c);
            object[] args = new object[p.Length + 1];
            args[0] = c;
            for (int i = 0; i < p.Length; i++)
            {
                args[i + 1] = p[i].Eval(c);
            }
            return f.Call(args);
        }

        static FunctionCallExpr()
        {
            Expression.RegisterParser("FUNCCALL", tree =>
            {
                Expression name = tree.GetChild("REF").ParseExpr();
                List<Expression> l = new List<Expression>();
                for (int i = 0; i < tree.GetChild("LIST").ChildCount; i++)
                {
                    l.Add(tree.GetChild("LIST").GetChild(i).ParseExpr());
                }
                return new FunctionCallExpr(name, l.ToArray());
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class SetExpr: Expression
    {
        IEnumerable<Expression> set;

        public SetExpr(IEnumerable<Expression> set)
        {
            this.set = new List<Expression>(set);
        }

        public override object Eval(IReadContext c)
        {
            var ret = new List<object>();
            foreach (var item in set)
            {
                ret.Add(item.Eval(c));
            }
            return ret;
        }

        static SetExpr()
        {
            Expression.RegisterParser("LIT_SET", tree =>
            {
                var l = new List<Expression>();
                for (int i = 0; i < tree.ChildCount; i++)
                {
                    l.Add(Expression.Parse(tree.GetChild(i)));
                }
                return new SetExpr(l);
            });
        }

        public override string ToString()
        {
            return "[...]";
        }
    }
}

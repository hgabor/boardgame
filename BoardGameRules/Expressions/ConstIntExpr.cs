using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class ConstIntExpr: Expression
    {
        int i;

        public ConstIntExpr(int i)
        {
            this.i = i;
        }

        public override object Eval(IReadContext c)
        {
            return i;
        }

        static ConstIntExpr()
        {
            Expression.RegisterParser("LIT_INT", tree =>
            {
                int i = int.Parse(tree.GetOnlyChild().Text);
                return new ConstIntExpr(i);
            });
        }

        public override string ToString()
        {
            return i.ToString();
        }
    }
}

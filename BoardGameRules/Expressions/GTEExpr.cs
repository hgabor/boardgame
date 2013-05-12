using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class GTEExpr : Expression
    {
        Expression lhs, rhs;
        public GTEExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            int l = (int)lhs.Eval(c);
            int r = (int)rhs.Eval(c);
            return l >= r;
        }

        static GTEExpr()
        {
            Expression.RegisterBinaryParser(">=", (l, r) => new GTEExpr(l, r));
        }
    }
}

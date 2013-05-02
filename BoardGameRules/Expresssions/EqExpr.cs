using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class EqExpr:Expression
    {
        Expression lhs, rhs;
        public EqExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            return lhs.Eval(c).Equals(rhs.Eval(c));
        }

        static EqExpr()
        {
            Expression.RegisterBinaryParser("=", (l, r) => new EqExpr(l, r));
        }
    }
}

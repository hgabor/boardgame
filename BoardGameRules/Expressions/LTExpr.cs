using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class LTExpr : Expression
    {
        Expression lhs, rhs;
        public LTExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(IReadContext c)
        {
            int l = (int)lhs.Eval(c);
            int r = (int)rhs.Eval(c);
            return l < r;
        }

        static LTExpr()
        {
            Expression.RegisterBinaryParser("<", (l, r) => new LTExpr(l, r));
        }

        public override string ToString()
        {
            return string.Format("({0} < {1})", lhs, rhs);
        }
    }
}

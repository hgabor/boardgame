using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class LTEExpr : Expression
    {
        Expression lhs, rhs;
        public LTEExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            int l = (int)lhs.Eval(c);
            int r = (int)rhs.Eval(c);
            return l <= r;
        }

        static LTEExpr()
        {
            Expression.RegisterBinaryParser("<=", (l, r) => new LTEExpr(l, r));
        }

        public override string ToString()
        {
            return string.Format("({0} <= {1})", lhs, rhs);
        }
    }
}

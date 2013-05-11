using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expresssions
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
            // Handle null values
            return object.Equals(lhs.Eval(c), rhs.Eval(c));
        }

        static EqExpr()
        {
            Expression.RegisterBinaryParser("=", (l, r) => new EqExpr(l, r));
            Expression.RegisterBinaryParser("!=", (l, r) => new NotExpr(new EqExpr(l, r)));
        }
    }
}

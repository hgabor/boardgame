using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class AndExpr: Expression
    {
        private Expression lhs;
        private Expression rhs;

        public AndExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            bool l = (bool)lhs.Eval(c);
            // Lazy evaluation
            if (l == false) return false;

            return rhs.Eval(c);
        }

        static AndExpr()
        {
            Expression.RegisterBinaryParser("And", (lhs, rhs) =>
            {
                return new AndExpr(lhs, rhs);
            });
        }


    }
}

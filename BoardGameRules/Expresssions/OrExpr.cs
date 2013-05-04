using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expresssions
{
    class OrExpr: Expression
    {
        private Expression lhs;
        private Expression rhs;

        public OrExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            bool l = (bool)lhs.Eval(c);
            // Lazy evaluation
            if (l == true) return true;

            return rhs.Eval(c);
        }

        static OrExpr()
        {
            Expression.RegisterBinaryParser("Or", (lhs, rhs) =>
            {
                return new OrExpr(lhs, rhs);
            });
        }


    }
}

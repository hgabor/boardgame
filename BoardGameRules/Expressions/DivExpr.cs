using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class DivExpr: Expression
    {
        private Expression lhs;
        private Expression rhs;
        public DivExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(IReadContext c)
        {
            return (int)lhs.Eval(c) / (int)rhs.Eval(c);
        }

        static DivExpr()
        {
            Expression.RegisterBinaryParser("/", (lhs, rhs) =>
            {
                return new DivExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} / {1})", lhs, rhs);
        }
    }
}

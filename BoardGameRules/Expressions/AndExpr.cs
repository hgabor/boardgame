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

        public override object Eval(GameState state, IReadContext c)
        {
            bool l = (bool)lhs.Eval(state, c);
            // Lazy evaluation
            if (l == false) return false;

            return rhs.Eval(state, c);
        }

        static AndExpr()
        {
            Expression.RegisterBinaryParser("And", (lhs, rhs) =>
            {
                return new AndExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} And {1})", lhs, rhs);
        }
    }
}

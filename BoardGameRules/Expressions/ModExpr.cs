using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class ModExpr: Expression
    {
        private Expression lhs;
        private Expression rhs;
        public ModExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(GameState state, IReadContext c)
        {
            return (int)lhs.Eval(state, c) % (int)rhs.Eval(state, c);
        }

        static ModExpr()
        {
            Expression.RegisterBinaryParser("%", (lhs, rhs) =>
            {
                return new ModExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} % {1})", lhs, rhs);
        }
    }
}

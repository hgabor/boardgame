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

        public override object Eval(IReadContext c)
        {
            return (int)lhs.Eval(c) % (int)rhs.Eval(c);
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

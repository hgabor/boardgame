using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expresssions
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

        public override object Eval(Context c)
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

    }
}

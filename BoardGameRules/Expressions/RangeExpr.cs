using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class RangeExpr: Expression
    {
        Expression lhs, rhs;

        public RangeExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(GameState state, IReadContext c)
        {
            int i1 = (int)lhs.Eval(state, c);
            int i2 = (int)rhs.Eval(state, c);
            return Enumerable.Range(i1, i2 - i1 + 1);
        }

        // Register parser
        static RangeExpr()
        {
            Expression.RegisterParser("..", tree =>
            {
                if (tree.ChildCount != 2) throw new InvalidGameException(".. operator must have exactly 2 children!");
                Expression lhs = Expression.Parse(tree.GetChild(0));
                Expression rhs = Expression.Parse(tree.GetChild(1));
                return new RangeExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} .. {1})", lhs, rhs);
        }
    }
}

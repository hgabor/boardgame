using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class SubExpr: Expression
    {
                Expression lhs, rhs;

        public SubExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(Context c)
        {
            int i1 = (int)lhs.Eval(c);
            int i2 = (int)rhs.Eval(c);
            return i1 - i2;
        }

        // Register parser
        static SubExpr()
        {
            Expression.RegisterParser("-", tree =>
            {
                if (tree.ChildCount != 2) throw new InvalidGameException("- operator must have exactly 2 children!");
                Expression lhs = Expression.Parse(tree.GetChild(0));
                Expression rhs = Expression.Parse(tree.GetChild(1));
                return new SubExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} - {1})", lhs, rhs);
        }
    }
}

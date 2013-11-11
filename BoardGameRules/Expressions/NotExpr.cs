using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class NotExpr: Expression
    {
        Expression expr;
        public NotExpr(Expression expr)
        {
            this.expr = expr;
        }

        public override object Eval(IReadContext c)
        {
            bool result = (bool)expr.Eval(c);
            return !result;
        }

        static NotExpr()
        {
            Expression.RegisterParser("Not", tree =>
            {
                if (tree.ChildCount != 1) throw new InvalidGameException("Not operator must have only one child!");
                return new NotExpr(tree.GetOnlyChild().ParseExpr());
            });
        }

        public override string ToString()
        {
            return string.Format("(Not {0})", expr);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;

namespace Level14.BoardGameRules.Statements
{
    class ExprStatement: Statement
    {
        Expression e;

        public ExprStatement(Expression e)
        {
            this.e = e;
        }

        public override ControlFlow Run(Context c)
        {
            e.Eval(c);
            return ControlFlow.Next;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
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

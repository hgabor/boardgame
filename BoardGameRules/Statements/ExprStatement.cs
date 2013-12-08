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

        public override ControlFlow Run(GameState state, Context c)
        {
            e.Eval(state, c);
            return ControlFlow.Next;
        }

        public override string ToString()
        {
            return e.ToString() + ";";
        }
    }
}

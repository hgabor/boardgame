using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;

namespace Level14.BoardGameRules.Statements
{
    class IfStatement : Statement
    {
        Expression condition;
        Statement action;

        public IfStatement(Expression condition, Statement action)
        {
            this.condition = condition;
            this.action = action;
        }

        public override ControlFlow Run(GameState state, Context c)
        {
            object o = condition.Eval(state, c);
            bool ifResult = (bool)o;
            if (ifResult)
            {
                return action.Run(state, c);
            }
            return ControlFlow.Next;
        }

        public override string ToString()
        {
            return string.Format("If {0} Then\n{1}End\n", condition, action);
        }
    }
}

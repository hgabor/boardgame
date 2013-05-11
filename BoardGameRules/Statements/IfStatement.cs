using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
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

        public override ControlFlow Run(Context c)
        {
            object o = condition.Eval(c);
            bool ifResult = (bool)o;
            if (ifResult)
            {
                return action.Run(c);
            }
            return ControlFlow.Next;
        }
    }
}

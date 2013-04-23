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

        public override void Run(Context c)
        {
            bool ifResult = (bool)condition.Eval(c);
            if (ifResult)
            {
                action.Run(c);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Statements
{
    class AssignmentStatement: Statement
    {
        private Expression variable;
        private Expression value;

        public AssignmentStatement(Expression variable, Expression value)
        {
            this.variable = variable;
            this.value = value;
        }

        public override ControlFlow Run(Context c)
        {
            Closure cl = (Closure)variable.Eval(c);
            object newValue = value.Eval(c);
            cl.SetVariable(newValue);
            return ControlFlow.Next;
        }
    }
}

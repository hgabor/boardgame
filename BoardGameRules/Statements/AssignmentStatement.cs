using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;

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

        public override ControlFlow Run(GameState state, Context c)
        {
            Closure cl = (Closure)variable.Eval(state, c);
            object newValue = value.Eval(state, c);
            cl.SetVariable(newValue);
            return ControlFlow.Next;
        }

        public override string ToString()
        {
            return string.Format("{0} := {1};\n", variable, value);
        }
    }
}

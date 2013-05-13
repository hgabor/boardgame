using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;

namespace Level14.BoardGameRules.Statements
{
    class ReturnStatement : Statement
    {
        private Expression exp;

        public ReturnStatement(Expression exp)
        {
            this.exp = exp;
        }

        public override ControlFlow Run(Context c)
        {
            object retval = exp.Eval(c);
            c.SetVariable("_Return", retval);
            return ControlFlow.Return;
        }

        public override string ToString()
        {
            return string.Format("Return {0};\n", exp);
        }
    }
}

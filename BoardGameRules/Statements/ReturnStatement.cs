﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}

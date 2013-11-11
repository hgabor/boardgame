﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class GTExpr : Expression
    {
        Expression lhs, rhs;
        public GTExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(IReadContext c)
        {
            int l = (int)lhs.Eval(c);
            int r = (int)rhs.Eval(c);
            return l > r;
        }

        static GTExpr()
        {
            Expression.RegisterBinaryParser(">", (l, r) => new GTExpr(l, r));
        }

        public override string ToString()
        {
            return string.Format("({0} > {1})", lhs, rhs);
        }
    }
}

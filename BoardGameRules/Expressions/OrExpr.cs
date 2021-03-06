﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class OrExpr: Expression
    {
        private Expression lhs;
        private Expression rhs;

        public OrExpr(Expression lhs, Expression rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override object Eval(GameState state, IReadContext c)
        {
            bool l = (bool)lhs.Eval(state, c);
            // Lazy evaluation
            if (l == true) return true;

            return rhs.Eval(state, c);
        }

        static OrExpr()
        {
            Expression.RegisterBinaryParser("Or", (lhs, rhs) =>
            {
                return new OrExpr(lhs, rhs);
            });
        }

        public override string ToString()
        {
            return string.Format("({0} Or {1})", lhs, rhs);
        }
    }
}

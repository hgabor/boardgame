using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class MemberAccessExpr: Expression
    {
        Expression parent;
        Expression member;

        public MemberAccessExpr(Expression parent, Expression member)
        {
            this.parent = parent;
            this.member = member;
        }

        public override object Eval(GameState state, IReadContext c)
        {
            var parentObj = (IReadContext)parent.Eval(state, c);
            return member.Eval(state, parentObj);
        }

        static MemberAccessExpr()
        {
            Expression.RegisterBinaryParser("MEMBER_ACCESS", (parent, member) => new MemberAccessExpr(parent, member));
        }

        public override string ToString()
        {
            return string.Format("({0}.{1})", parent, member);
        }
    }
}

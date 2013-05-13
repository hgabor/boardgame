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

        public override object Eval(Context c)
        {
            Context parentObj = (Context)parent.Eval(c);
            return member.Eval(parentObj);
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

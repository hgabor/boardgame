using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class SelectExpr: Expression
    {
        Expression varName;
        Expression from;
        Expression where;

        public SelectExpr(Expression varName, Expression from, Expression where = null)
        {
            this.varName = varName;
            this.from = from;
            this.where = where;
        }

        public override object Eval(Context c)
        {
            IEnumerable<object> set = (IEnumerable<object>)from.Eval(c);
            Set newSet = new Set();
            foreach (var o in set)
            {
                Context oContext = (Context)o;
                if (where == null)
                {
                    newSet.Add(varName.Eval(oContext));
                }
                else
                {
                    bool result = (bool)where.Eval(new MultiParentContext(c.Game, oContext, c));
                    if (result)
                    {
                        newSet.Add(varName.Eval(oContext));
                    }
                }
            }
            return newSet;
        }

        static SelectExpr()
        {
            Expression.RegisterParser("SELECT", tree =>
            {
                Expression varName = Expression.Parse(tree.GetChild("REF"));
                Expression from = Expression.Parse(tree.GetChild("SELECT_FROM").GetOnlyChild());
                Expression where = null;
                if (tree.HasChild("SELECT_WHERE"))
                {
                    where = Expression.Parse(tree.GetChild("SELECT_WHERE").GetOnlyChild());
                }
                return new SelectExpr(varName, from, where);
            });
        }
    }
}

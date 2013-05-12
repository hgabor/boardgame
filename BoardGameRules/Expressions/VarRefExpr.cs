using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class VarRefExpr: Expression
    {
        string name;
        public VarRefExpr(string name)
        {
            this.name = name;
        }

        public override object Eval(Context c)
        {
            return new Closure(c, name);
        }
        
        static VarRefExpr()
        {
            Expression.RegisterParser("VAR_REF", tree =>
            {
                string name = tree.GetOnlyChild().Text;
                return new VarRefExpr(name);
            });
        }

        public override string ToString()
        {
            return name;
        }
    }
}

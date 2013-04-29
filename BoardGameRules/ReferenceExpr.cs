using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class ReferenceExpr: Expression
    {
        string name;
        public ReferenceExpr(string name)
        {
            this.name = name;
        }

        public override object Eval(Context c)
        {
            object o = c.GetVariable(name);
            if (o == null) throw new InvalidGameException(string.Format("Variable {0} does not exist!", name));
            return o;
        }
        
        static ReferenceExpr()
        {
            Expression.RegisterParser("REF", tree =>
            {
                string name = tree.GetOnlyChild().Text;
                return new ReferenceExpr(name);
            });
        }

        public override string ToString()
        {
            return name;
        }
    }
}

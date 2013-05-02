using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class PlaceHolderExpr: Expression
    {
        public override object Eval(Context c)
        {
            return PlaceHolderValue.Value;
        }

        static PlaceHolderExpr()
        {
            Expression.RegisterParser("_", tree =>
            {
                return new PlaceHolderExpr();
            });
        }

        public override string ToString()
        {
            return "_";
        }
    }
}

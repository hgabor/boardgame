using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class CoordExpr: Expression
    {
        Expression[] coords;

        public CoordExpr(params Expression[] expr)
        {
            coords = (Expression[])expr.Clone();
        }

        public override object Eval(IReadContext c)
        {
            int[] newCoords = new int[coords.Length];
            bool[] placeholder = new bool[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                object v = coords[i].Eval(c);
                if (v is PlaceHolderValue) {
                    placeholder[i] = true;
                }
                else {
                    newCoords[i] = (int)v;
                }
            }
            return new Coords(newCoords, placeholder);

        }

        static CoordExpr()
        {
            Expression.RegisterParser("LIT_COORDS", tree =>
            {
                Expression[] e = new Expression[tree.ChildCount];
                for (int i = 0; i < tree.ChildCount; i++)
                {
                    e[i] = Expression.Parse(tree.GetChild(i));
                }
                return new CoordExpr(e);
            });
        }

        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(", ", (object[])coords));
        }
    }
}

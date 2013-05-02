using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class PlayerRefExpr: Expression
    {
        Expression playerID;

        public PlayerRefExpr(Expression playerID)
        {
            this.playerID = playerID;
        }

        public override object Eval(Context c)
        {
            return c.GetPlayer((int)playerID.Eval(c));
        }

        static PlayerRefExpr()
        {
            Expression.RegisterParser("PLAYERREF", tree =>
            {
                Expression playerRef = tree.GetOnlyChild().ParseExpr();
                return new PlayerRefExpr(playerRef);
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class PlayerRefExpr: Expression
    {
        Expression playerID;

        public PlayerRefExpr(Expression playerID)
        {
            this.playerID = playerID;
        }

        public override object Eval(IReadContext c)
        {
            return c.Game.GetPlayer((int)playerID.Eval(c));
        }

        static PlayerRefExpr()
        {
            Expression.RegisterParser("PLAYERREF", tree =>
            {
                Expression playerRef = tree.GetOnlyChild().ParseExpr();
                return new PlayerRefExpr(playerRef);
            });
        }

        public override string ToString()
        {
            return string.Format("Player({0})", playerID);
        }
    }
}

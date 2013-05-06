using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public partial class Game
    {
        private static class M
        {
            public static int Count(Context ctx, IEnumerable<object> set)
            {
                return set.Count();
            }

            public static bool IsEmpty(Context ctx, Coords c)
            {
                return ctx.Game.board[c] == null;
            }

            public static void Lose(Context ctx, Player p)
            {
                p.Lost = true;
            }

            public static object Min(Context ctx, IEnumerable<object> set)
            {
                return set.Min();
            }

            public static void NextPlayer(Context ctx, Player p)
            {
                ctx.Game.OverrideNextPlayer = p;
            }

            public static void RemovePiece(Context ctx, Coords c)
            {
                ctx.Game.board.TryRemove(c);
            }

            public static void Win(Context ctx, Player p)
            {
                p.Won = true;
            }
        }

        private void RegisterMethod(params string[] names)
        {
            foreach (var name in names)
            {
                this.globalContext.SetVariable(name, new Function(typeof(M).GetMethod(name)));
            }
        }

        partial void InitGlobals()
        {
            RegisterMethod(
                "Count",
                "IsEmpty",
                "Lose",
                "Min",
                "NextPlayer",
                "RemovePiece",
                "Win"
                );
        }
    }
}

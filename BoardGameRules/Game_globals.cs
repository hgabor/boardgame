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
            public static Coords ChoosePlace(Context ctx, string title, IEnumerable<object> set) {
                var coordSet = set.Select(o => (Coords)o);
                return Game.ChoosePlace(title, ctx.Game.CurrentPlayer, coordSet);
            }

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

            public static object Min(Context ctx, IEnumerable<object> set) {
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

            public static void Win(Context ctx, Player p) {
                p.Won = true;
            }
        }

        void RegisterMethod(string name)
        {
            this.globalContext.SetVariable(name, new Function(typeof(M).GetMethod(name)));
        }

        partial void InitGlobals()
        {
            RegisterMethod("Count");
            RegisterMethod("IsEmpty");
            RegisterMethod("Lose");
            RegisterMethod("Min");
            RegisterMethod("NextPlayer");
            RegisterMethod("RemovePiece");
            RegisterMethod("Win");
        }
    }
}

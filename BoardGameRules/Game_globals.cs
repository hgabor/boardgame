﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Level14.BoardGameRules.Expressions;
using System.Runtime.CompilerServices;

namespace Level14.BoardGameRules
{
    public partial class Game
    {
        private static class M
        {
            public static Piece ChoosePiece(Context ctx, Player p, IEnumerable<object> set)
            {
                if (set.Count() == 0)
                {
                    return null;
                }
                var pieces = set.OfType<Piece>();
                int currentPlayerID = ctx.Game.currentPlayer;
                ctx.Game.currentPlayer = p.ID - 1;
                Piece pi = ctx.Game.AskForPiece(pieces);
                ctx.Game.currentPlayer = currentPlayerID;
                return pi;
            }

            public static int Count(Context ctx, IEnumerable<object> set)
            {
                return set.Count();
            }

            public static void DebugBreak(Context ctx)
            {
                System.Diagnostics.Debugger.Break();
            }

            public static void DebugPrint(Context ctx, object o)
            {
                Console.WriteLine("DEBUG: {0}", o);
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

            public static Piece PieceAt(Context ctx, Coords c)
            {
                return ctx.Game.board[c];
            }

            public static void RemovePiece(Context ctx, object toRemove)
            {
                Coords c = toRemove as Coords;
                if (c != null)
                {
                    ctx.Game.board.TryRemove(c);
                    return;
                }
                Piece p = toRemove as Piece;
                if (p != null)
                {
                    ctx.Game.board.TryRemove(p.GetPosition());
                }
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
                this.globalContext.SetVariable(name, new PredefinedFunction(typeof(M).GetMethod(name)));
            }
        }

        partial void InitGlobals()
        {
            RegisterMethod(
                "ChoosePiece",
                "Count",
                "DebugBreak",
                "DebugPrint",
                "IsEmpty",
                "Lose",
                "Min",
                "NextPlayer",
                "PieceAt",
                "RemovePiece",
                "Win"
                );
        }

        class GlobalContext : Context
        {
            internal GlobalContext(Game g) : base(g) { }

            internal override object GetVariable(string name)
            {
                switch (name)
                {
                    case "CurrentPlayer":
                        return Game.CurrentPlayer;
                    case "False":
                        return false;
                    case "None":
                        return null;
                    case "Pieces":
                        var pieces = Game.board.GetPiecesWithoutCoords();
                        foreach (var p in Game.players)
                        {
                            pieces = pieces.Union(p.GetOffboard());
                        }
                        return pieces;
                    case "True":
                        return true;
                    default:
                        return base.GetVariable(name);
                }
            }
        }

        // Run ALL static constructors...
        private static void RegisterSubClasses()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(Expression).IsAssignableFrom(type))
                {
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
            }
        }
        static Game()
        {
            RegisterSubClasses();
        }
    }
}

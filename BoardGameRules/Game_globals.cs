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
                int currentPlayerID = ctx.GameState.CurrentPlayerID;
                ctx.GameState.CurrentPlayerID = p.ID - 1;
                Piece pi = ctx.Game.AskForPiece(pieces);
                ctx.GameState.CurrentPlayerID = currentPlayerID;
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
                return ctx.Game.board.PieceAt(c, ctx.GameState.CurrentPlayer) == null;
            }

            public static void Lose(Context ctx, Player p)
            {
                p.Lost = true;
            }

            public static object Max(Context ctx, IEnumerable<object> set)
            {
                return set.Max();
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
                return ctx.Game.board.PieceAt(c, ctx.GameState.CurrentPlayer);
            }

            public static void Place(Context ctx, string piecetype, Coords c)
            {
                Piece p = new Piece(piecetype, ctx.GameState.CurrentPlayer, ctx.GameState);
                ctx.Game.board.TryPut(c, p, ctx.GameState.CurrentPlayer);
            }

            public static void RemovePiece(Context ctx, object toRemove)
            {
                Coords c = toRemove as Coords;
                if (c != null)
                {
                    ctx.Game.board.TryRemove(c, ctx.GameState.CurrentPlayer);
                    return;
                }
                Piece p = toRemove as Piece;
                if (p != null)
                {
                    ctx.Game.board.TryRemove(p.GetPosition(ctx.GameState.CurrentPlayer), ctx.GameState.CurrentPlayer);
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
                "Max",
                "Min",
                "NextPlayer",
                "PieceAt",
                "Place",
                "RemovePiece",
                "Win"
                );
        }

        class GlobalContext : Context
        {
            internal GlobalContext(GameState g) : base(g) { }

            internal override object GetVariable(string name)
            {
                switch (name)
                {
                    case "AllowedMoves":
                        return Game.allowedMoves;
                    case "CurrentPlayer":
                        return GameState.CurrentPlayer;
                    case "False":
                        return false;
                    case "None":
                        return null;
                    case "OppositePlayer":
                        if (Game.PlayerCount != 2) return null;
                        return Game.GetPlayer(3 - GameState.CurrentPlayer.ID);
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

            internal override void SetVariable(string name, object value)
            {
                if (name == "AllowedMoves")
                {
                    var newValObj = value as IEnumerable<object>;
                    if (newValObj == null) {
                        throw new InvalidGameException("AllowedMoves must be a set of move rules!");
                    }
                    var newVal = newValObj.OfType<MoveDefinition>();
                    Game.allowedMoves = newVal;
                }
                else
                {
                    base.SetVariable(name, value);
                }
            }

            protected override bool HasVariable(string name)
            {
                return name == "AllowedMoves" || base.HasVariable(name);
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

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
            public static int Abs(Context ctx, int i)
            {
                return Math.Abs(i);
            }

            public static IEnumerable<object> AddToSet(Context ctx, IEnumerable<object> set, object obj)
            {
                return set.Union(new[] { obj });
            }

            public static Piece ChoosePiece(Context ctx, Player p, IEnumerable<object> set)
            {
                if (set.Count() == 0)
                {
                    return null;
                }
                var pieces = set.OfType<Piece>();
                int currentPlayerID = ctx.GameState.CurrentPlayerID;
                ctx.GameState.CurrentPlayerID = p.ID - 1;
                Piece pi = ctx.Game.AskForPiece(ctx.GameState, pieces);
                ctx.GameState.CurrentPlayerID = currentPlayerID;
                return pi;
            }

            public static bool Contains(Context ctx, IEnumerable<object> set, object obj)
            {
                return set.Contains(obj);
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

            public static IEnumerable<object> GetMatching(Context ctx, Coords from, Coords target, int direction, RegExp.Pattern pattern)
            {
                IEnumerable<Coords> c;
                if (pattern.CaptureAll(ctx.GameState, target, from, RegExp.Pattern.Direction.Any, out c))
                {
                    return c;
                }
                else
                {
                    return new List<object>();
                }
            }

            public static void ForEach(Context ctx, IEnumerable<object> set, ICallable func)
            {
                foreach (var obj in set)
                {
                    func.Call(ctx.GameState, ctx, new[] { obj });
                }
            }


            public static bool IsEmpty(Context ctx, Coords c)
            {
                return ctx.Game.board.PieceAt(ctx.GameState, c, ctx.GameState.CurrentPlayer) == null;
            }

            public static void Lose(Context ctx, Player p)
            {
                p.Lost = true;
            }

            public static bool Match(Context ctx, Coords from, Coords target, RegExp.Pattern.Direction direction, RegExp.Pattern pattern)
            {
                return pattern.Match(ctx.GameState, target, from, direction);
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
                ctx.GameState.OverrideNextPlayer = p;
            }

            public static Piece PieceAt(Context ctx, Coords c)
            {
                return ctx.Game.board.PieceAt(ctx.GameState, c, ctx.GameState.CurrentPlayer);
            }

            public static void Place(Context ctx, string piecetype, Coords c)
            {
                Piece p = new Piece(piecetype, ctx.GameState.CurrentPlayer, ctx.Game);
                ctx.Game.board.TryPut(ctx.GameState, c, p, ctx.GameState.CurrentPlayer);
            }

            public static void RemovePiece(Context ctx, object toRemove)
            {
                Coords c = toRemove as Coords;
                if (c != null)
                {
                    ctx.Game.board.TryRemove(ctx.GameState, c, ctx.GameState.CurrentPlayer);
                    return;
                }
                Piece p = toRemove as Piece;
                if (p != null)
                {
                    ctx.Game.board.TryRemove(ctx.GameState, p.GetPosition(ctx.GameState, ctx.GameState.CurrentPlayer), ctx.GameState.CurrentPlayer);
                }
            }

            public static void Win(Context ctx, Player p)
            {
                p.Won = true;
            }
        }

        private void RegisterMethod(GameState state, params string[] names)
        {
            foreach (var name in names)
            {
                state.GlobalContext.SetVariable(name, new PredefinedFunction(typeof(M).GetMethod(name)));
            }
        }

        partial void InitGlobals(GameState state)
        {
            RegisterMethod(
                state,
                "Abs",
                "AddToSet",
                "ChoosePiece",
                "Contains",
                "Count",
                "DebugBreak",
                "DebugPrint",
                "GetMatching",
                "ForEach",
                "IsEmpty",
                "Lose",
                "Match",
                "Max",
                "Min",
                "NextPlayer",
                "PieceAt",
                "Place",
                "RemovePiece",
                "Win"
                );
        }

        internal class GlobalContext : Context
        {
            internal GlobalContext(GameState g) : base(g) { }

            protected GlobalContext(GlobalContext ctx, GameState g) : base(ctx, g) { }

            public override object GetVariable(GameState state, string name)
            {
                switch (name)
                {
                    case "AllowedMoves":
                        return GameState.AllowedMoves;
                    case "CurrentGameState":
                        return state;
                    case "CurrentPlayer":
                        return GameState.CurrentPlayer;
                    case "Direction_Any":
                        return RegExp.Pattern.Direction.Any;
                    case "Direction_Diagonal":
                        return RegExp.Pattern.Direction.Diagonal;
                    case "Direction_Straight":
                        return RegExp.Pattern.Direction.Straight;
                    case "False":
                        return false;
                    case "GameStates":
                        return Game.oldStates;//.Union(new[] {state});
                    case "None":
                        return null;
                    case "OppositePlayer":
                        if (Game.PlayerCount != 2) return null;
                        return Game.GetPlayer(3 - GameState.CurrentPlayer.ID);
                    case "Pieces":
                        var pieces = Game.board.GetPiecesWithoutCoords(GameState);
                        foreach (var p in Game.players)
                        {
                            pieces = pieces.Union(p.GetOffboard(GameState));
                        }
                        return pieces;
                    case "True":
                        return true;
                    default:
                        return base.GetVariable(state, name);
                }
            }

            public override void SetVariable(string name, object value)
            {
                if (name == "AllowedMoves")
                {
                    var newValObj = value as IEnumerable<object>;
                    if (newValObj == null) {
                        throw new InvalidGameException("AllowedMoves must be a set of move rules!");
                    }
                    var newVal = newValObj.OfType<MoveDefinition>();
                    GameState.AllowedMoves = newVal;
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

            internal override Context Clone(GameState newState)
            {
                return new GlobalContext(this, newState);
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

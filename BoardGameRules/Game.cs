﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System.Reflection;
using System.Runtime.CompilerServices;
using Level14.BoardGameRules.Expressions;
using Level14.BoardGameRules.Statements;

namespace Level14.BoardGameRules
{
    public partial class Game
    {
        public int PlayerCount { get { return players.Count; } }
        public Player GetCurrentPlayer(GameState state)
        {
            return players[state.CurrentPlayerID];
        }

        public Coords Size { get { return board.Size; } }

        private List<string> pieceTypes = new List<string>();
        private List<Player> players = new List<Player>();

        private List<MoveRule> moveRules = new List<MoveRule>();

        private List<GameState> oldStates = new List<GameState>();

        internal IEnumerable<Player> EnumeratePlayers()
        {
            return players.AsReadOnly();
        }

        public IEnumerable<string> PieceTypes
        {
            get
            {
                return pieceTypes.AsReadOnly();
            }
        }

        public bool GameOver
        {
            get
            {
                foreach (var p in players)
                {
                    if (p.Won || p.Tied || p.Lost) return true;
                }
                return false;
            }
        }

        public IEnumerable<Player> Winners
        {
            get
            {
                // If there are winners, they win
                return players.FindAll(p => p.Won);
            }
        }

        internal Context GetGlobalContext(GameState state) { return state.GlobalContext; }

        internal Player GetPlayer(int i)
        {
            return players[i - 1];
        }

        private Board board;

        internal bool IsValidPiece(string type)
        {
            return pieceTypes.Contains(type);
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces(GameState state)
        {
            return GetPieces(state, null);
        }

        internal IEnumerable<KeyValuePair<Coords, Piece>> GetPieces(GameState state, Player asker)
        {
            return board.GetPieces(state, null); // FIXME: null should be asker, possible bug?
        }

        partial void InitGlobals(GameState state);

        public Game(string rules, out GameState firstState)
        {
            try
            {
                var input = new ANTLRStringStream(rules);
                var lexer = new BoardGameLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new BoardGameParser(tokens);

                var root = parser.parse();

                var errors = parser.GetErrors();

                if (errors.Length > 0)
                {
                    var msg = string.Join("\n", errors);
                    throw new InvalidGameException(string.Format("Number of errors: {0} \n\n{1}", errors.Length, msg));
                }

                var currentState = new GameState(this);
                currentState.GlobalContext = new GlobalContext(currentState);
                InitGlobals(currentState);

                CommonTree t = (CommonTree)root.Tree;

                int playerCount = (int)t.GetChild("SETTINGS").GetChild("PlayerCount").GetOnlyChild().ParseExpr().Eval(currentState, currentState.GlobalContext);
                for (int i = 0; i < playerCount; i++) players.Add(new Player(i+1));

                var size = (Coords)t.GetChild("SETTINGS").GetChild("BoardDimensions").GetOnlyChild().ParseExpr().Eval(currentState, currentState.GlobalContext);
                board = new Board(size);

                for (int i = 0; i < t.GetChild("SETTINGS").GetChild("PieceTypes").ChildCount; i++)
                {
                    string pt = t.GetChild("SETTINGS").GetChild("PieceTypes").GetChild(i).Text;
                    pieceTypes.Add(pt);
                    currentState.GlobalContext.SetVariable(pt, pt);
                }

                bool mirrorEvents = false;
                bool mirrorMoves = false;

                if (t.GetChild("SETTINGS").HasChild("RulesOfPlayer2"))
                {
                    string value = t.GetChild("SETTINGS").GetChild("RulesOfPlayer2").GetOnlyChild().Text;
                    switch (value)
                    {
                        case "SameAsPlayer1":
                            mirrorEvents = true;
                            mirrorMoves = true;
                            break;
                        case "RotationOfPlayer1":
                            mirrorEvents = true;
                            mirrorMoves = true;
                            board.Transformation = (asker, coord) =>
                            {
                                if (asker == GetPlayer(2))
                                {
                                    return new Coords(new int[] { Size[0] + 1 - coord[0], Size[0] + 1 - coord[1] }, coord.PlaceHolders);
                                }
                                else
                                {
                                    return coord;
                                }
                            };
                            break;
                        case "MirrorOfPlayer1":
                            mirrorEvents = true;
                            mirrorMoves = true;
                            board.Transformation = (asker, coord) =>
                            {
                                if (asker == GetPlayer(2))
                                {
                                    return new Coords(new int[] { coord[0], Size[0] + 1 - coord[1] }, coord.PlaceHolders);
                                }
                                else
                                {
                                    return coord;
                                }
                            };
                            break;
                        default:
                            throw new InvalidGameException(string.Format("Unsupported rule transformation: {0}", value));
                    }
                }

                if (t.HasChild("FUNCDEFLIST"))
                {
                    var functions = t.GetChild("FUNCDEFLIST");
                    for (int i = 0; i < functions.ChildCount; i++)
                    {
                        var funcNode = functions.GetChild(i);
                        System.Diagnostics.Debug.Assert(funcNode.Text == "FUNCDEF");
                        string name = funcNode.GetChild("REF").GetOnlyChild().Text;
                        var paramList = new List<string>();
                        for (int j = 0; j < funcNode.GetChild("PARAMLIST").ChildCount; j++)
                        {
                            paramList.Add(funcNode.GetChild("PARAMLIST").GetChild(j).Text);
                        }
                        var body = funcNode.GetChild("STATEMENTS").ParseStmtList();
                        var func = new UserFunction(paramList.ToArray(), body);
                        currentState.GlobalContext.SetVariable(name, func);
                    }
                }

                if (t.HasChild("INIT"))
                {
                    var stmt = t.GetChild("INIT").GetChild("STATEMENTS").ParseStmtList();
                    stmt.Run(currentState, currentState.GlobalContext);
                }

                if (t.HasChild("STARTINGBOARD"))
                {
                    for (int i = 0; i < t.GetChild("STARTINGBOARD").ChildCount; i++)
                    {
                        var ch = t.GetChild("STARTINGBOARD").GetChild(i);
                        switch (ch.Text)
                        {
                            case "Invalid":
                                board.Valid = Board.RuleType.Invalid;
                                for (int j = 0; j < ch.ChildCount; j++)
                                {
                                    board.AddRule(ch.GetChild(j).ParseExpr());
                                }
                                break;
                            case "Valid":
                                board.Valid = Board.RuleType.Valid;
                                for (int j = 0; j < ch.ChildCount; j++)
                                {
                                    board.AddRule(ch.GetChild(j).ParseExpr());
                                }
                                break;
                            case "STARTINGPIECES":
                                int ownerInt = (int)ch.GetChild("PLAYERREF").GetOnlyChild().ParseExpr().Eval(currentState, currentState.GlobalContext);
                                Player owner = GetPlayer(ownerInt);
                                currentState.CurrentPlayerID = ownerInt - 1;

                                for (int j = 0; j < ch.GetChild("LIST").ChildCount; j++)
                                {
                                    ITree pieceNode = ch.GetChild("LIST").GetChild(j);
                                    string type = pieceNode.Text;
                                    if (!IsValidPiece(type))
                                    {
                                        throw new InvalidGameException(pieceNode.FileCoords() + " - Invalid piece type '" + type + "'");
                                    }
                                    Coords coords = null;
                                    if (pieceNode.HasChild("LIT_COORDS"))
                                    {
                                        coords = (Coords)pieceNode.GetChild("LIT_COORDS").ParseExpr().Eval(currentState, currentState.GlobalContext);
                                    }
                                    else if (pieceNode.HasChild("Offboard"))
                                    { /* Do nothing */ }
                                    else
                                    {
                                        throw new Exception("Cannot happen");
                                    }
                                    var p = new Piece(type, owner, this);
                                    if (coords == null)
                                    {
                                        currentState.CurrentPlayer.AddOffboard(currentState, p);
                                    }
                                    else if (!board.TryPut(currentState, coords, p, null))
                                    {
                                        throw new InvalidGameException(pieceNode.FileCoords() + " - Invalid coords for '" + type + "'");
                                    }
                                    if (pieceNode.HasChild("TAG"))
                                    {
                                        string tag = pieceNode.GetChild("TAG").GetOnlyChild().Text;
                                        currentState.GlobalContext.SetVariable(tag, p);
                                    }
                                }

                                break;
                        }
                    }
                }

                if (t.HasChild("PATTERNS"))
                {
                    for (int i = 0; i < t.GetChild("PATTERNS").ChildCount; i++)
                    {
                        var patternNode = t.GetChild("PATTERNS").GetChild(i);
                        var patternName = patternNode.GetChild("PATTERNNAME").GetOnlyChild().Text;
                        var elements = new List<RegExp.PatternElement>();
                        for (int j = 0; j < patternNode.GetChild("PATTERNPARTS").ChildCount; j++)
                        {
                            var elementNode = patternNode.GetChild("PATTERNPARTS").GetChild(j);
                            RegExp.PatternElement element = new RegExp.PatternElement();
                            var predNode = elementNode.GetChild("PATTERNPREDICATE").GetOnlyChild();
                            if (predNode.Text == "Empty")
                            {
                                element.Predicate = (state, coords) => board.PieceAt(state, coords, state.CurrentPlayer) == null;
                            }
                            else
                            {
                                Expression expr = predNode.ParseExpr();
                                element.Predicate = (state, coords) =>
                                {
                                    Context ctx = new Context(state.GlobalContext);
                                    ctx.SetXYZ(coords, state.CurrentPlayer);
                                    var retVal = expr.Eval(state, ctx);
                                    Player pl = retVal as Player;
                                    if (pl != null)
                                    {
                                        Piece piece = board.PieceAt(state, coords, state.CurrentPlayer);
                                        if (piece == null) return false;
                                        return piece.Owner == pl;
                                    }
                                    throw new NotSupportedException(string.Format("Unsupported pattern predicate: {0}", retVal));
                                };
                            }
                            var countNode = elementNode.GetChild("PATTERNCOUNT").GetOnlyChild();
                            if (countNode.Text == "Target")
                            {
                                element.IsTarget = true;
                                element.Count = new int[] { 1 };
                            }
                            else
                            {
                                Context ctx = new Context(currentState.GlobalContext);
                                Expression expr = countNode.ParseExpr();
                                object count = expr.Eval(currentState, ctx);
                                if (count is int)
                                {
                                    element.Count = new int[] { (int)count };
                                }
                                else if (count is IEnumerable<int>)
                                {
                                    element.Count = (IEnumerable<int>)count;
                                }
                                else if (count is IEnumerable<object>)
                                {
                                    element.Count = from obj in (IEnumerable<object>)count
                                                    select (int)obj;
                                }
                                else
                                {
                                    throw new InvalidGameException("Invalid value for pattern range.");
                                }
                            }
                            elements.Add(element);
                        }
                        var pattern = new RegExp.Pattern(elements);
                        currentState.GlobalContext.SetVariable(patternName, pattern);
                    }
                }

                ITree moveRoot = t.GetChild("MOVES");
                if (mirrorMoves)
                {
                    moveRoot.Rewrite(TreeHelpers.RewriteMirrorPlayer);
                }

                for (int i = 0; i < t.GetChild("MOVES").ChildCount; i++)
                {
                    var moveNode = t.GetChild("MOVES").GetChild(i);
                    string piece = moveNode.Text;
                    var moveOpNode = moveNode.GetChild("OP_MOVE");
                    var moveFromNode = moveOpNode.GetChild("MOVE_FROM");

                    CoordExpr moveFrom = null;
                    CoordExpr moveTo = null;

                    if (!moveFromNode.HasChild("Offboard"))
                    {
                        moveFrom = (CoordExpr)moveFromNode.GetOnlyChild().ParseExpr();
                    }
                    var moveToNode = moveOpNode.GetChild("MOVE_TO");
                    if (!moveToNode.HasChild("Offboard"))
                    {
                        moveTo = (CoordExpr)moveToNode.GetOnlyChild().ParseExpr();
                    }
                    var moveOptionsNode = moveOpNode.GetChild("MOVE_OPTIONS");
                    bool emptyTarget = moveOptionsNode.HasChild("Empty");

                    string label = null;
                    if (moveOpNode.HasChild("Label"))
                    {
                        label = moveOpNode.GetChild("Label").GetOnlyChild().Text;
                        currentState.GlobalContext.SetVariable(label, label);
                    }

                    Expression condition = null;
                    if (moveOpNode.HasChild("IF"))
                    {
                        condition = moveOpNode.GetChild("IF").GetOnlyChild().ParseExpr();
                    }

                    Statement stmt = null;
                    if (moveOpNode.HasChild("STATEMENTS"))
                    {
                        stmt = moveOpNode.GetChild("STATEMENTS").ParseStmtList();
                    }

                    var rule = new MoveRule(piece, moveFrom, moveTo, emptyTarget, label, condition, stmt, this);
                    moveRules.Add(rule);
                }

                if (mirrorEvents)
                {
                    t.GetChild("EVENTS").Rewrite(TreeHelpers.RewriteMirrorPlayer);
                }

                for (int i = 0; i < t.GetChild("EVENTS").ChildCount; i++)
                {
                    var eventNode = t.GetChild("EVENTS").GetChild(i);
                    var stmt = eventNode.GetChild("STATEMENTS").ParseStmtList();

                    // Register event for all players
                    for (int eventI = 0; eventI < eventNode.GetChild("EVENTTYPES").ChildCount; eventI++)
                    {
                        var eventTypeNode = eventNode.GetChild("EVENTTYPES").GetChild(eventI);
                        Player p = (Player)eventTypeNode.GetChild("PLAYERREF").ParseExpr().Eval(currentState, new Context(currentState));
                        string eventType = eventTypeNode.GetChild(1).Text;
                        switch (eventType)
                        {
                            case "CannotMove":
                                p.AddCannotMove(stmt);
                                break;
                            case "FinishedMove":
                                p.AddPostMove(stmt);
                                break;
                            case "BeforeMove":
                                p.AddPreMove(stmt);
                                break;
                            default:
                                throw new InvalidGameException("Invalid event: " + eventType);
                        }
                    }

                }

                currentState.CurrentPlayerID = 0;
                PreMoveActions(currentState);
                firstState = currentState;
            }
            catch (InvalidGameException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidGameException(e);
            }
        }

        public GameState TryMakeMoveFromOffboard(GameState oldState, Piece piece, Coords to)
        {
            GameState newState = oldState.Clone();
            Context ctx = new Context(newState.GlobalContext);
            ctx.SetVariable("to", Transform(to, newState.CurrentPlayer));
            ctx.SetVariable("NewGameState", newState);

            // piece cannot be null
            if (piece == null) return null;
            // Cannot move opponent's piece
            if (piece.Owner != newState.CurrentPlayer) return null;

            bool isInlist = EnumerateMovesFromOffboard(newState, piece).Any(
                md => md.From == null && Coords.Match(md.To, to) && md.PieceType == piece.Type);
            if (!isInlist) return null;


            if (!MoveIsValidGlobal(newState, null, to, ctx)) return null;


            Piece oppPiece = board.PieceAt(newState, to, null);
            foreach (var rule in moveRules)
            {
                // Only offboard rules here
                if (!rule.OffboardRule) continue;

                if (!MoveIsValidForRule(newState, rule, piece, null, to, ctx)) continue;


                // Move is valid
                rule.RunAction(newState, ctx);

                // Perform the move
                if (!newState.CurrentPlayer.RemoveOffBoard(newState, piece))
                {
                    // Original piece was removed, should not happen!
                    throw new InvalidGameException("Cannot move from offboard, piece " + oppPiece.ToString() + " was removed!");
                }
                if (!board.TryPut(newState, to, piece, null))
                {
                    // Piece was not captured
                    throw new InvalidGameException("Cannot move to " + to.ToString() + ", piece " + oppPiece.ToString() + " is in the way!");
                }

                ctx.SetXYZ(to, newState.CurrentPlayer);

                // Move was performed
                oldStates.Add(oldState);

                newState.CurrentPlayer.PostMove(newState, ctx);
                PostMoveActions(newState);

                return newState;
            }
            // No suitable rule found.
            return null;
        }

        internal bool MoveIsValidGlobal(GameState currentState, Coords from, Coords to, Context ctx)
        {
            if (from != null && !board.IsValidPlace(currentState, from)) return false;
            if (to != null && !board.IsValidPlace(currentState, to)) return false;
            if (from == null && to == null) return false;

            if (from != null)
            {
                Piece piece = board.PieceAt(currentState, from, null);

                // Cannot move empty square
                if (piece == null) return false;

                // Cannot move opponent's piece
                if (piece.Owner != currentState.CurrentPlayer) return false;

                // Cannot stay in place
                // Why not? :)
                //if (Coords.Match(from, to)) return false;
            }

            return true;
        }

        internal bool MoveIsValidForRule(GameState currentState, MoveRule rule, Piece piece, Coords from, Coords to, Context ctx)
        {
            if (from != null) piece = board.PieceAt(currentState, from, currentState.CurrentPlayer);
            Piece oppPiece = board.PieceAt(currentState, to, currentState.CurrentPlayer);

            // Rule must be applicable to current piece
            if (rule.PieceType != piece.Type) return false;

            // Target should be empty
            if (rule.TargetMustBeEmpty && oppPiece != null) return false;

            if (rule.Condition != null)
            {
                bool cond = (bool)rule.Condition.Eval(currentState, ctx);
                if (!cond) return false;
            }

            if (rule.From == null)
            {
                if (!currentState.CurrentPlayer.GetOffboard(currentState).Contains(piece)) return false;
                //var toT = board.Transformation(CurrentPlayer, to);
                ///var toTExpr = board.Transformation(CurrentPlayer, (Coords)rule.To.Eval(ctx));
                if (!Coords.Match(to, (Coords)rule.To.Eval(currentState, ctx))) return false;
            }
            else if (rule.To == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                // Check coords
                //var fromT = board.Transformation(CurrentPlayer, from);
                //var toT = board.Transformation(CurrentPlayer, to);
                //var fromTExpr = board.Transformation(CurrentPlayer, (Coords)rule.From.Eval(ctx));
                //var toTExpr = board.Transformation(CurrentPlayer, (Coords)rule.To.Eval(ctx));

                if (!Coords.Match(from, (Coords)rule.From.Eval(currentState, ctx))) return false;
                if (!Coords.Match(to, (Coords)rule.To.Eval(currentState, ctx))) return false;
            }
            return true;
        }

        public GameState TryMakeMove(GameState oldState, Coords from, Coords to) {
            GameState newState = oldState.Clone();
            Context ctx = new Context(newState.GlobalContext);
            // Special vars x, y are from coordinates
            ctx.SetXYZ(from, newState.CurrentPlayer);
            ctx.SetVariable("from", Transform(from, newState.CurrentPlayer));
            ctx.SetVariable("to", Transform(to, newState.CurrentPlayer));
            ctx.SetVariable("NewGameState", newState);


            if (!MoveIsValidGlobal(newState, from, to, ctx)) return null;

            Piece piece = board.PieceAt(newState, from, null);
            Piece oppPiece = board.PieceAt(newState, to, null);

            bool isInlist = EnumerateMovesFromCoord(newState, from).Any(
                md => Coords.Match(md.From, from) && Coords.Match(md.To, to));
            if (!isInlist) return null;


            foreach (var rule in moveRules)
            {
                // No offboard rules here
                if (rule.OffboardRule) continue;

                var fromT = Transform(from, newState.CurrentPlayer);
                var toT = Transform(to, newState.CurrentPlayer);
                if (!MoveIsValidForRule(newState, rule, null, fromT, toT, ctx)) continue;

                // Move is valid
                rule.RunAction(newState, ctx);

                // Perform the move
                piece = board.PieceAt(newState, from, null);
                if (!board.TryRemove(newState, from, null))
                {
                    // Original piece was removed, should not happen!
                    throw new InvalidGameException("Cannot move from " + from.ToString() + ", piece " + oppPiece.ToString() + " was removed!");
                }
                if (!board.TryPut(newState, to, piece, null))
                {
                    // Piece was not captured
                    throw new InvalidGameException("Cannot move to " + to.ToString() + ", piece " + oppPiece.ToString() + " is in the way!");
                }

                ctx.SetXYZ(to, newState.CurrentPlayer);

                // Move was performed
                oldStates.Add(oldState);

                newState.CurrentPlayer.PostMove(newState, ctx);
                PostMoveActions(newState);

                return newState;
            }
            // No suitable rule found.
            return null;
        }

        private void PreMoveActions(GameState currentState)
        {
            // If current player cannot move...
            int counter = 0;
            while (!GameOver)
            {
                counter++;
                if (counter > 100)
                {
                    throw new InvalidGameException("Game went into a loop, where no one can move, lose or win.");
                }
                currentState.AllowedMoves = null;
                currentState.AllowedMoves = EnumeratePossibleMoves(currentState);
                currentState.CurrentPlayer.PreMove(currentState, Context.NewLocal(currentState));
                if (currentState.AllowedMoves.Count() == 0)
                {
                    currentState.CurrentPlayer.CannotMove(currentState, currentState.GlobalContext);
                    // In most games if the player cannot move, he loses
                    if (currentState.CurrentPlayer.Lost) return;
                    else
                    {
                        // But not always, so we should try the next player
                        currentState.CurrentPlayerID = (currentState.CurrentPlayerID + 1) % PlayerCount;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void PostMoveActions(GameState state)
        {
            int startPlayer = state.CurrentPlayerID;

            do
            {
                state.CurrentPlayerID = (state.CurrentPlayerID + 1) % PlayerCount;
                if (state.CurrentPlayerID == startPlayer)
                {
                    break;
                }
            } while (state.CurrentPlayer.Lost);

            if (state.OverrideNextPlayer != null)
            {
                state.CurrentPlayerID = state.OverrideNextPlayer.ID - 1;
                state.OverrideNextPlayer = null;
            }

            PreMoveActions(state);
        }

        public IEnumerable<MoveDefinition> EnumeratePossibleMoves(GameState state)
        {
            HashSet<MoveDefinition> moves = new HashSet<MoveDefinition>();

            foreach (var p in state.CurrentPlayer.GetOffboard(state))
            {
                foreach (var c in board.EnumerateCoords(state, null))
                {
                    foreach (var rule in this.moveRules)
                    {
                        if (rule.From != null) continue;
                        Context ctx = new Context(state.GlobalContext);
                        ctx.SetXYZ(c, state.CurrentPlayer);
                        var cT = Transform(c, state.CurrentPlayer);
                        ctx.SetVariable("to", cT);
                        if (MoveIsValidGlobal(state, null, c, ctx) && MoveIsValidForRule(state, rule, p, null, cT, ctx))
                        {
                            moves.Add(new MoveDefinition(pieceType: p.Type, label: rule.Label, from: null, to: c, game: this));
                        }
                    }
                }
            }
            foreach (var from in board.EnumerateCoords(state, null))
            {
                foreach (var to in board.EnumerateCoords(state, null))
                {
                    foreach (var rule in this.moveRules)
                    {
                        if (rule.OffboardRule) continue;
                        Context ctx = new Context(state.GlobalContext);
                        ctx.SetXYZ(from, state.CurrentPlayer);
                        var fromT = Transform(from, state.CurrentPlayer);
                        var toT = Transform(to, state.CurrentPlayer);
                        ctx.SetVariable("from", fromT);
                        ctx.SetVariable("to", toT);
                        if (MoveIsValidGlobal(state, from, to, ctx) && MoveIsValidForRule(state, rule, null, fromT, toT, ctx))
                        {
                            moves.Add(new MoveDefinition(pieceType: board.PieceAt(state, from, null).Type, label: rule.Label, from: from, to: to, game: this));
                        }
                    }
                }
            }
            return state.AllowedMoves == null ? moves : moves.Intersect(state.AllowedMoves);
        }

        public IEnumerable<MoveDefinition> EnumerateMovesFromCoord(GameState state, Coords c)
        {
            var ret = EnumeratePossibleMoves(state).Where(md => md.From != null && Coords.Match(c, md.From));
            return ret;
        }

        public IEnumerable<MoveDefinition> EnumerateMovesFromOffboard(GameState state, Piece p)
        {
            var ret = EnumeratePossibleMoves(state).Where(md => md.From == null && md.PieceType == p.Type);
            return ret;
        }

        public delegate Piece SelectPieceFunction (GameState state, IEnumerable<Piece> pieces);
        private SelectPieceFunction selectPieceFunction;
        public void SetSelectPieceFunction(SelectPieceFunction s)
        {
            this.selectPieceFunction = s;
        }
        internal Piece AskForPiece(GameState state, IEnumerable<Piece> pieces)
        {
            if (selectPieceFunction == null) throw new InvalidGameException("SetSelectPieceFunction must be called to allow piece selection!");
            return selectPieceFunction(state, pieces);
        }

        internal Coords Transform(Coords c, Player asker)
        {
            return board.Transformation(asker, c);
        }
    }
}

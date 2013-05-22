using System;
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
        private int currentPlayer;
        public Player CurrentPlayer { get { return players[currentPlayer]; } }
        internal Player OverrideNextPlayer { get; set; }

        public Coords Size { get { return board.Size; } }

        private List<string> pieceTypes = new List<string>();
        private List<Player> players = new List<Player>();

        private IEnumerable<MoveDefinition> allowedMoves;
        private List<MoveRule> moveRules = new List<MoveRule>();

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

        private Context globalContext;
        internal Context GetGlobalContext() { return globalContext; }

        internal Player GetPlayer(int i)
        {
            return players[i - 1];
        }

        private Board board;

        internal bool IsValidPiece(string type)
        {
            return pieceTypes.Contains(type);
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces()
        {
            return GetPieces(null);
        }

        internal IEnumerable<KeyValuePair<Coords, Piece>> GetPieces(Player asker)
        {
            return board.GetPieces(null);
        }

        partial void InitGlobals();

        public Game(string rules)
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

                globalContext = new GlobalContext(this);
                InitGlobals();

                CommonTree t = (CommonTree)root.Tree;

                int playerCount = (int)t.GetChild("SETTINGS").GetChild("PlayerCount").GetOnlyChild().ParseExpr().Eval(globalContext);
                for (int i = 0; i < playerCount; i++) players.Add(new Player(i+1));

                var size = (Coords)t.GetChild("SETTINGS").GetChild("BoardDimensions").GetOnlyChild().ParseExpr().Eval(globalContext);
                board = new Board(size, this);

                for (int i = 0; i < t.GetChild("SETTINGS").GetChild("PieceTypes").ChildCount; i++)
                {
                    string pt = t.GetChild("SETTINGS").GetChild("PieceTypes").GetChild(i).Text;
                    pieceTypes.Add(pt);
                    globalContext.SetVariable(pt, pt);
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
                        globalContext.SetVariable(name, func);
                    }
                }

                if (t.HasChild("INIT"))
                {
                    var stmt = t.GetChild("INIT").GetChild("STATEMENTS").ParseStmtList();
                    stmt.Run(globalContext);
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
                                int ownerInt = (int)ch.GetChild("PLAYERREF").GetOnlyChild().ParseExpr().Eval(globalContext);
                                Player owner = GetPlayer(ownerInt);
                                currentPlayer = ownerInt - 1;

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
                                        coords = (Coords)pieceNode.GetChild("LIT_COORDS").ParseExpr().Eval(globalContext);
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
                                        CurrentPlayer.AddOffboard(p);
                                    }
                                    else if (!board.TryPut(coords, p, null))
                                    {
                                        throw new InvalidGameException(pieceNode.FileCoords() + " - Invalid coords for '" + type + "'");
                                    }
                                    if (pieceNode.HasChild("TAG"))
                                    {
                                        string tag = pieceNode.GetChild("TAG").GetOnlyChild().Text;
                                        globalContext.SetVariable(tag, p);
                                    }
                                }

                                break;
                        }
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
                        globalContext.SetVariable(label, label);
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
                        Player p = (Player)eventTypeNode.GetChild("PLAYERREF").ParseExpr().Eval(new Context(this));
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

                currentPlayer = 0;
                PreMoveActions();
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

        public bool TryMakeMoveFromOffboard(Piece piece, Coords to)
        {
            Context ctx = new Context(this.globalContext);
            ctx.SetVariable("to", Transform(to, CurrentPlayer));

            // piece cannot be null
            if (piece == null) return false;
            // Cannot move opponent's piece
            if (piece.Owner != CurrentPlayer) return false;

            bool isInlist = EnumerateMovesFromOffboard(piece).Any(
                md => md.From == null && Coords.Match(md.To, to) && md.PieceType == piece.Type);
            if (!isInlist) return false;


            if (!MoveIsValidGlobal(null, to, ctx)) return false;


            Piece oppPiece = board.PieceAt(to, null);
            foreach (var rule in moveRules)
            {
                // Only offboard rules here
                if (!rule.OffboardRule) continue;

                if (!MoveIsValidForRule(rule, piece, null, to, ctx)) continue;


                // Move is valid
                rule.RunAction(ctx);

                // Perform the move
                if (!CurrentPlayer.RemoveOffBoard(piece))
                {
                    // Original piece was removed, should not happen!
                    throw new InvalidGameException("Cannot move from offboard, piece " + oppPiece.ToString() + " was removed!");
                }
                if (!board.TryPut(to, piece, null))
                {
                    // Piece was not captured
                    throw new InvalidGameException("Cannot move to " + to.ToString() + ", piece " + oppPiece.ToString() + " is in the way!");
                }

                ctx.SetXYZ(to, CurrentPlayer);
                CurrentPlayer.PostMove(ctx);
                PostMoveActions();

                // Move was performed
                return true;
            }
            // No suitable rule found.
            return false;
        }

        internal bool MoveIsValidGlobal(Coords from, Coords to, Context ctx)
        {
            if (from != null && !board.IsValidPlace(from)) return false;
            if (to != null && !board.IsValidPlace(to)) return false;
            if (from == null && to == null) return false;

            if (from != null)
            {
                Piece piece = board.PieceAt(from, null);

                // Cannot move empty square
                if (piece == null) return false;

                // Cannot move opponent's piece
                if (piece.Owner != CurrentPlayer) return false;

                // Cannot stay in place
                if (Coords.Match(from, to)) return false;
            }

            return true;
        }

        internal bool MoveIsValidForRule(MoveRule rule, Piece piece, Coords from, Coords to, Context ctx)
        {
            if (from != null) piece = board.PieceAt(from, CurrentPlayer);
            Piece oppPiece = board.PieceAt(to, CurrentPlayer);

            // Rule must be applicable to current piece
            if (rule.PieceType != piece.Type) return false;

            // Target should be empty
            if (rule.TargetMustBeEmpty && oppPiece != null) return false;

            if (rule.Condition != null)
            {
                bool cond = (bool)rule.Condition.Eval(ctx);
                if (!cond) return false;
            }

            if (rule.From == null)
            {
                if (!CurrentPlayer.GetOffboard().Contains(piece)) return false;
                //var toT = board.Transformation(CurrentPlayer, to);
                ///var toTExpr = board.Transformation(CurrentPlayer, (Coords)rule.To.Eval(ctx));
                if (!Coords.Match(to, (Coords)rule.To.Eval(ctx))) return false;
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

                if (!Coords.Match(from, (Coords)rule.From.Eval(ctx))) return false;
                if (!Coords.Match(to, (Coords)rule.To.Eval(ctx))) return false;
            }
            return true;
        }

        public bool TryMakeMove(Coords from, Coords to) {

            Context ctx = new Context(this.globalContext);
            // Special vars x, y are from coordinates
            ctx.SetXYZ(from, CurrentPlayer);
            ctx.SetVariable("from", Transform(from, CurrentPlayer));
            ctx.SetVariable("to", Transform(to, CurrentPlayer));


            if (!MoveIsValidGlobal(from, to, ctx)) return false;

            Piece piece = board.PieceAt(from, null);
            Piece oppPiece = board.PieceAt(to, null);

            bool isInlist = EnumerateMovesFromCoord(from).Any(
                md => Coords.Match(md.From, from) && Coords.Match(md.To, to));
            if (!isInlist) return false;


            foreach (var rule in moveRules)
            {
                // No offboard rules here
                if (rule.OffboardRule) continue;

                var fromT = Transform(from, CurrentPlayer);
                var toT = Transform(to, CurrentPlayer);
                if (!MoveIsValidForRule(rule, null, fromT, toT, ctx)) continue;

                // Move is valid
                rule.RunAction(ctx);

                // Perform the move
                piece = board.PieceAt(from, null);
                if (!board.TryRemove(from, null))
                {
                    // Original piece was removed, should not happen!
                    throw new InvalidGameException("Cannot move from " + from.ToString() + ", piece " + oppPiece.ToString() + " was removed!");
                }
                if (!board.TryPut(to, piece, null))
                {
                    // Piece was not captured
                    throw new InvalidGameException("Cannot move to " + to.ToString() + ", piece " + oppPiece.ToString() + " is in the way!");
                }

                ctx.SetXYZ(to, CurrentPlayer);
                CurrentPlayer.PostMove(ctx);
                PostMoveActions();

                // Move was performed
                return true;
            }
            // No suitable rule found.
            return false;
        }

        private void PreMoveActions()
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
                allowedMoves = null;
                allowedMoves = EnumeratePossibleMoves();
                CurrentPlayer.PreMove(Context.NewLocal(this));
                if (allowedMoves.Count() == 0)
                {
                    CurrentPlayer.CannotMove(globalContext);
                    // In most games if the player cannot move, he loses
                    if (CurrentPlayer.Lost) return;
                    else
                    {
                        // But not always, so we should try the next player
                        currentPlayer = (currentPlayer + 1) % PlayerCount;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void PostMoveActions()
        {
            int startPlayer = currentPlayer;

            do
            {
                currentPlayer = (currentPlayer + 1) % PlayerCount;
                if (currentPlayer == startPlayer)
                {
                    break;
                }
            } while (CurrentPlayer.Lost);

            if (OverrideNextPlayer != null)
            {
                currentPlayer = OverrideNextPlayer.ID - 1;
                OverrideNextPlayer = null;
            }

            PreMoveActions();
        }

        public IEnumerable<MoveDefinition> EnumeratePossibleMoves()
        {
            HashSet<MoveDefinition> moves = new HashSet<MoveDefinition>();

            foreach (var p in CurrentPlayer.GetOffboard())
            {
                foreach (var c in board.EnumerateCoords(null))
                {
                    foreach (var rule in this.moveRules)
                    {
                        if (rule.From != null) continue;
                        Context ctx = new Context(globalContext);
                        ctx.SetXYZ(c, CurrentPlayer);
                        var cT = Transform(c, CurrentPlayer);
                        ctx.SetVariable("to", cT);
                        if (MoveIsValidGlobal(null, c, ctx) && MoveIsValidForRule(rule, p, null, cT, ctx))
                        {
                            moves.Add(new MoveDefinition(pieceType: p.Type, label: rule.Label, from: null, to: c, game: this));
                        }
                    }
                }
            }
            foreach (var from in board.EnumerateCoords(null))
            {
                foreach (var to in board.EnumerateCoords(null))
                {
                    foreach (var rule in this.moveRules)
                    {
                        if (rule.OffboardRule) continue;
                        Context ctx = new Context(globalContext);
                        ctx.SetXYZ(from, CurrentPlayer);
                        var fromT = Transform(from, CurrentPlayer);
                        var toT = Transform(to, CurrentPlayer);
                        ctx.SetVariable("from", fromT);
                        ctx.SetVariable("to", toT);
                        if (MoveIsValidGlobal(from, to, ctx) && MoveIsValidForRule(rule, null, fromT, toT, ctx))
                        {
                            moves.Add(new MoveDefinition(pieceType: board.PieceAt(from, null).Type, label: rule.Label, from: from, to: to, game: this));
                        }
                    }
                }
            }
            return allowedMoves == null ? moves : moves.Intersect(allowedMoves);
        }

        public IEnumerable<MoveDefinition> EnumerateMovesFromCoord(Coords c)
        {
            var ret = EnumeratePossibleMoves().Where(md => md.From != null && Coords.Match(c, md.From));
            return ret;
        }

        public IEnumerable<MoveDefinition> EnumerateMovesFromOffboard(Piece p)
        {
            var ret = EnumeratePossibleMoves().Where(md => md.From == null && md.PieceType == p.Type);
            return ret;
        }

        public delegate Piece SelectPieceFunction (IEnumerable<Piece> pieces);
        private SelectPieceFunction selectPieceFunction;
        public void SetSelectPieceFunction(SelectPieceFunction s)
        {
            this.selectPieceFunction = s;
        }
        internal Piece AskForPiece(IEnumerable<Piece> pieces)
        {
            if (selectPieceFunction == null) throw new InvalidGameException("SetSelectPieceFunction must be called to allow piece selection!");
            return selectPieceFunction(pieces);
        }

        internal Coords Transform(Coords c, Player asker)
        {
            return board.Transformation(asker, c);
        }
    }
}

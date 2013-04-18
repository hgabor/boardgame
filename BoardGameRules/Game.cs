using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Level14.BoardGameRules
{
    public class Game
    {
        public int PlayerCount { get { return players.Count; } }
        private int currentPlayer;
        public Player CurrentPlayer { get { return players[currentPlayer]; } }

        public Coords Size { get { return board.Size; } }

        private List<string> pieceTypes = new List<string>();
        private List<Player> players = new List<Player>();
        private List<MoveRule> moveRules = new List<MoveRule>();
        private Player GetPlayer(int i)
        {
            return players[i - 1];
        }

        private Board board;

        public bool IsValidPiece(string type)
        {
            return pieceTypes.Contains(type);
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces()
        {
            return board.GetPieces();
        }

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

                // Set player count

                CommonTree t = (CommonTree)root.Tree;

                int playerCount = (int)t.GetChild("SETTINGS").GetChild("PlayerCount").GetOnlyChild().ParseExpr().Eval(null);
                for (int i = 0; i < playerCount; i++) players.Add(new Player(i+1));

                var size = (Coords)t.GetChild("SETTINGS").GetChild("BoardDimensions").GetOnlyChild().ParseExpr().Eval(null);
                board = new Board(size);

                for (int i = 0; i < t.GetChild("SETTINGS").GetChild("PieceTypes").ChildCount; i++)
                {
                    pieceTypes.Add(t.GetChild("SETTINGS").GetChild("PieceTypes").GetChild(i).Text);
                }

                if (t.HasChild("STARTINGBOARD"))
                {
                    for (int i = 0; i < t.GetChild("STARTINGBOARD").ChildCount; i++)
                    {
                        var ch = t.GetChild("STARTINGBOARD").GetChild(i);
                        switch (ch.Text)
                        {
                            case "STARTINGPIECES":
                                int ownerInt = (int)ch.GetChild("PLAYERREF").GetOnlyChild().ParseExpr().Eval(null);
                                Player owner = GetPlayer(ownerInt);

                                for (int j = 0; j < ch.GetChild("LIST").ChildCount; j++)
                                {
                                    ITree pieceNode = ch.GetChild("LIST").GetChild(j);
                                    string type = pieceNode.Text;
                                    if (!IsValidPiece(type))
                                    {
                                        throw new InvalidGameException(pieceNode.FileCoords() + " - Invalid piece type '" + type + "'");
                                    }
                                    Coords coords = (Coords)pieceNode.GetOnlyChild().ParseExpr().Eval(null);
                                    var p = new Piece(type, owner);
                                    if (!board.TryPut(coords, p))
                                    {
                                        throw new InvalidGameException(pieceNode.FileCoords() + " - Invalid coords for '" + type + "'");
                                    }
                                }

                                break;
                        }
                    }
                }

                for (int i = 0; i < t.GetChild("MOVES").ChildCount; i++)
                {
                    var moveNode = t.GetChild("MOVES").GetChild(i);
                    string piece = moveNode.Text;
                    var moveOpNode = moveNode.GetChild("OP_MOVE");
                    var moveFromNode = moveOpNode.GetChild("MOVE_FROM");
                    CoordExpr moveFrom = (CoordExpr)moveFromNode.GetOnlyChild().ParseExpr();

                    var moveToNode = moveOpNode.GetChild("MOVE_TO");
                    CoordExpr moveTo = (CoordExpr)moveToNode.GetOnlyChild().ParseExpr();

                    var moveOptionsNode = moveOpNode.GetChild("MOVE_OPTIONS");
                    bool emptyTarget = moveOptionsNode.HasChild("Empty");

                    var rule = new MoveRule(piece, moveFrom, moveTo, emptyTarget);
                    moveRules.Add(rule);
                }

                currentPlayer = 0;
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

        public bool TryMakeMove(Coords from, Coords to) {
            // Coords are valid?
            if (!board.IsInsideBoard(from)) return false;
            if (!board.IsInsideBoard(to)) return false;

            Context ctx = new Context();
            // Special vars x, y are from coordinates
            ctx.SetVariable("x", from[0]);
            ctx.SetVariable("y", from[1]);

            Piece piece = board[from];
            // Cannot move empty square
            if (piece == null) return false;
            // Cannot move opponent's piece
            if (piece.Owner != CurrentPlayer) return false;
            Piece oppPiece = board[to];

            foreach (var rule in moveRules)
            {
                // Rule must be applicable to current piece
                if (rule.PieceType != piece.Type) continue;

                // Target should be empty
                if (rule.TargetMustBeEmpty && oppPiece != null) continue;

                // Check coords
                if (!Coords.Match(from, (Coords)rule.From.Eval(ctx))) continue;
                if (!Coords.Match(to, (Coords)rule.To.Eval(ctx))) continue;

                // Move is valid
                // TODO: Here we should execute post-move actions.

                // Perform the move
                piece = board[from];
                if (!board.TryRemove(from))
                {
                    // Original piece was removed, should not happen!
                    throw new InvalidGameException("Cannot move from " + from.ToString() + ", piece " + oppPiece.ToString() + " was removed!");
                }
                if (!board.TryPut(to, piece))
                {
                    // Piece was not captured
                    throw new InvalidGameException("Cannot move to " + to.ToString() + ", piece " + oppPiece.ToString() + " is in the way!");
                }

                PostMoveActions();

                // Move was performed
                return true;
            }
            // No suitable rule found.
            return false;
        }

        private void PostMoveActions()
        {
            currentPlayer = (currentPlayer + 1) % PlayerCount;
        }

        static Game()
        {
            RegisterSubClasses();
        }
    }
}

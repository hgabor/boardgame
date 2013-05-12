using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;

namespace Level14.BoardGameRules
{
    class Board
    {
        public delegate Coords CoordTransformation(Player askingPlayer, Coords c);

        private Coords IdentityTransformation(Player p, Coords c)
        {
            return c;
        }

        Dictionary<Coords, Piece> board = new Dictionary<Coords, Piece>();
        public Coords Size { get; private set; }
        private Coords lowerLeft;
        private Game game;
        public CoordTransformation Transformation { get; set; }

        public Board(Coords size, Game game)
        {
            this.Size = size;
            lowerLeft = new Coords(Array.ConvertAll(size.ToInt32Array(), i => 1));
            this.game = game;
            this.Valid = RuleType.Invalid;
            Transformation = IdentityTransformation;
        }

        public bool TryPut(Coords cIn, Piece p, Player asker)
        {
            Coords c = Transformation(asker, cIn);

            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsValidPlace(c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (board.ContainsKey(c)) return false;
            board.Add(c, p);
            return true;
        }
        public bool TryRemove(Coords cIn, Player asker)
        {
            Coords c = Transformation(asker, cIn);

            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsValidPlace(c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (!board.ContainsKey(c)) return false;
            board.Remove(c);
            return true;
        }

        public enum RuleType { Valid, Invalid }
        public RuleType Valid { get; set; }

        private List<Expression> ruleList = new List<Expression>();

        public void AddRule(Expression exp)
        {
            ruleList.Add(exp);
        }

        private bool IsValidByRules(Coords c)
        {
            Context ctx = new Context(game);
            ctx.SetXYZ(c, null);
            if (Valid == RuleType.Valid)
            {
                // We must match at least one rule
                foreach (var exp in ruleList)
                {
                    bool b = (bool)exp.Eval(ctx);
                    if (b) return true;
                }
                return false;
            }
            else
            {
                // We mustn't match any rule
                foreach (var exp in ruleList)
                {
                    bool b = (bool)exp.Eval(ctx);
                    if (b) return false;
                }
                return true;
            }
        }

        public bool IsValidPlace(Coords c)
        {
            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");

            for (int i = 0; i < c.Dimension; i++)
            {
                if (c[i] < 1 || c[i] > Size[i]) return false;
            }
            return IsValidByRules(c);
        }

        public Piece PieceAt(Coords c, Player asker)
        {
            Piece p;
            if (board.TryGetValue(Transformation(asker, c), out p)) return p;
            return null;
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces(Player asker)
        {
            var newDict = board.Select(kvp => new KeyValuePair<Coords, Piece>(Transformation(asker, kvp.Key), kvp.Value));
            return newDict;
        }

        public IEnumerable<Piece> GetPiecesWithoutCoords()
        {
            return board.Values.AsEnumerable();
        }

        private void PossibleCoordsToArray(int[] coords, int dimension, Player asker, List<Coords> outList)
        {
            if (dimension == Size.Dimension)
            {
                Coords c = new Coords(coords);
                if (IsValidByRules(c))
                {
                    outList.Add(Transformation(asker, c));
                }
                return;
            }
            for (int i = 1; i <= Size[dimension]; i++)
            {
                var newCoords = new List<int>(coords);
                newCoords.Add(i);
                PossibleCoordsToArray(newCoords.ToArray(), dimension + 1, asker, outList);
            }
        }

        public IEnumerable<Coords> EnumerateCoords(Player asker)
        {
            var coords = new List<Coords>();
            PossibleCoordsToArray(new int[0], 0, asker, coords);
            return coords;
        }
    }
}

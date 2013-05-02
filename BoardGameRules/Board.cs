using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class Board
    {
        Dictionary<Coords, Piece> board = new Dictionary<Coords, Piece>();
        public Coords Size { get; private set; }
        private Coords lowerLeft;
        private Game game;

        public Board(Coords size, Game game)
        {
            this.Size = size;
            lowerLeft = new Coords(Array.ConvertAll(size.ToInt32Array(), i => 1));
            this.game = game;
            this.Valid = RuleType.Invalid;
        }

        public bool TryPut(Coords c, Piece p)
        {
            if (c.PlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsValidPlace(c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (board.ContainsKey(c)) return false;
            board.Add(c, p);
            return true;
        }
        public bool TryRemove(Coords c)
        {
            if (c.PlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
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
            ctx.SetVariable("x", c[0]);
            ctx.SetVariable("y", c[1]);
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
            if (c.PlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");

            for (int i = 0; i < c.Dimension; i++)
            {
                if (c[i] < 1 || c[i] > Size[i]) return false;
            }
            return IsValidByRules(c);
        }

        public Piece this[Coords c]
        {
            get
            {
                Piece p;
                if (board.TryGetValue(c, out p)) return p;
                return null;
            }
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces()
        {
            return board.AsEnumerable();
        }

        public IEnumerable<Piece> GetPiecesWithoutCoords()
        {
            return board.Values.AsEnumerable();
        }

        private void PossibleCoordsToArray(int[] coords, int dimension, List<Coords> outList)
        {
            if (dimension == Size.Dimension)
            {
                Coords c = new Coords(coords);
                if (IsValidByRules(c))
                {
                    outList.Add(c);
                }
                return;
            }
            for (int i = 1; i <= Size[dimension]; i++)
            {
                var newCoords = new List<int>(coords);
                newCoords.Add(i);
                PossibleCoordsToArray(newCoords.ToArray(), dimension + 1, outList);
            }
        }

        public IEnumerable<Coords> EnumerateCoords()
        {
            var coords = new List<Coords>();
            PossibleCoordsToArray(new int[0], 0, coords);
            return coords;
        }
    }
}

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

        public Coords Size { get; private set; }
        private Coords lowerLeft;
        public CoordTransformation Transformation { get; set; }

        public Board(Coords size)
        {
            this.Size = size;
            lowerLeft = new Coords(Array.ConvertAll(size.ToInt32Array(), i => 1));
            this.Valid = RuleType.Invalid;
            Transformation = IdentityTransformation;
        }

        public bool TryPut(GameState state, Coords cIn, Piece p, Player asker)
        {
            Coords c = Transformation(asker, cIn);

            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsValidPlace(state, c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (state.Board.ContainsKey(c)) return false;
            state.Board.Add(c, p);
            return true;
        }
        public bool TryRemove(GameState state, Coords cIn, Player asker)
        {
            Coords c = Transformation(asker, cIn);

            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsValidPlace(state, c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (!state.Board.ContainsKey(c)) return false;
            state.Board.Remove(c);
            return true;
        }

        public enum RuleType { Valid, Invalid }
        public RuleType Valid { get; set; }

        private List<Expression> ruleList = new List<Expression>();

        public void AddRule(Expression exp)
        {
            ruleList.Add(exp);
        }

        private bool IsValidByRules(GameState state, Coords c)
        {
            Context ctx = new Context(state);
            ctx.SetXYZ(c, null);
            if (Valid == RuleType.Valid)
            {
                // We must match at least one rule
                foreach (var exp in ruleList)
                {
                    bool b = (bool)exp.Eval(state, ctx);
                    if (b) return true;
                }
                return false;
            }
            else
            {
                // We mustn't match any rule
                foreach (var exp in ruleList)
                {
                    bool b = (bool)exp.Eval(state, ctx);
                    if (b) return false;
                }
                return true;
            }
        }

        public bool IsValidPlace(GameState state, Coords c)
        {
            if (c.IsPlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");

            for (int i = 0; i < c.Dimension; i++)
            {
                if (c[i] < 1 || c[i] > Size[i]) return false;
            }
            return IsValidByRules(state, c);
        }

        public Piece PieceAt(GameState state, Coords c, Player asker)
        {
            Piece p;
            if (state.Board.TryGetValue(Transformation(asker, c), out p)) return p;
            return null;
        }

        public IEnumerable<KeyValuePair<Coords, Piece>> GetPieces(GameState state, Player asker)
        {
            var newDict = state.Board.Select(kvp => new KeyValuePair<Coords, Piece>(Transformation(asker, kvp.Key), kvp.Value));
            return newDict;
        }

        public IEnumerable<Piece> GetPiecesWithoutCoords(GameState state)
        {
            return state.Board.Values.AsEnumerable();
        }

        private void PossibleCoordsToArray(GameState state, int[] coords, int dimension, Player asker, List<Coords> outList)
        {
            if (dimension == Size.Dimension)
            {
                Coords c = new Coords(coords);
                if (IsValidByRules(state, c))
                {
                    outList.Add(Transformation(asker, c));
                }
                return;
            }
            for (int i = 1; i <= Size[dimension]; i++)
            {
                var newCoords = new List<int>(coords);
                newCoords.Add(i);
                PossibleCoordsToArray(state, newCoords.ToArray(), dimension + 1, asker, outList);
            }
        }

        public IEnumerable<Coords> EnumerateCoords(GameState state, Player asker)
        {
            var coords = new List<Coords>();
            PossibleCoordsToArray(state, new int[0], 0, asker, coords);
            return coords;
        }
    }
}

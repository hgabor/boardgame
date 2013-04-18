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

        public Board(Coords size)
        {
            this.Size = size;
            lowerLeft = new Coords(Array.ConvertAll(size.ToInt32Array(), i => 1));
        }

        public bool TryPut(Coords c, Piece p)
        {
            if (c.PlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            if (!IsInsideBoard(c))
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
            if (!IsInsideBoard(c))
            {
                throw new ArgumentOutOfRangeException("Coords must be inside the board.");
            }
            if (!board.ContainsKey(c)) return false;
            board.Remove(c);
            return true;
        }
        public bool IsInsideBoard(Coords c)
        {
            if (c.PlaceHolder) throw new ArgumentOutOfRangeException("Non-placeholder coords needed.");
            // TODO: check for "special" coordinates

            for (int i = 0; i < c.Dimension; i++)
            {
                if (c[i] < 1 || c[i] > Size[i]) return false;
            }
            return true;
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
    }
}

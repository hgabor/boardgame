using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules;

namespace Level14.BoardGameGUI
{
    class CoordTransformer: ICoordTransformer
    {
        Dictionary<Coords, Coords> game2board = new Dictionary<Coords, Coords>();
        Dictionary<Coords, Coords> board2game = new Dictionary<Coords, Coords>();

        public CoordTransformer(Coords[] game, Coords[] board)
        {
            if (game.Length != board.Length) throw new ArgumentException("Lengths must be equal");

            for (int i = 0; i < game.Length; i++)
            {
                if (board[i].Dimension != 2) throw new ArgumentException("Board coords must be in 2D!");
                game2board.Add(game[i], board[i]);
                board2game.Add(board[i], game[i]);
            }
        }

        public Coords GameToBoard(Coords c)
        {
            Coords ret;
            if (game2board.TryGetValue(c, out ret))
            {
                return ret;
            }
            else
            {
                return new Coords(-1);
            }
        }

        public Coords BoardToGame(Coords c)
        {
            Coords ret;
            if (board2game.TryGetValue(c, out ret))
            {
                return ret;
            }
            else
            {
                return new Coords(-1);
            }
        }

        public bool IsValidBoardCoord(Coords c)
        {
            return board2game.ContainsKey(c);
        }
    }
}

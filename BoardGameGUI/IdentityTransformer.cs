using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameGUI
{
    class IdentityTransformer: ICoordTransformer
    {
        public BoardGameRules.Coords GameToBoard(BoardGameRules.Coords c)
        {
            return c;
        }

        public BoardGameRules.Coords BoardToGame(BoardGameRules.Coords c)
        {
            return c;
        }

        public bool IsValidBoardCoord(BoardGameRules.Coords c)
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules;

namespace Level14.BoardGameGUI
{
    interface ICoordTransformer
    {
        Coords GameToBoard(Coords c);
        Coords BoardToGame(Coords c);

        bool IsValidBoardCoord(Coords c);
    }
}

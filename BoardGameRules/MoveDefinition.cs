using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public struct MoveDefinition
    {
        public string PieceType;
        public Coords From;
        public Coords To;

        public override string ToString()
        {
            string from = From == null ? "Offboard" : From.ToString();
            string to = To == null ? "Offboard" : To.ToString();
            return string.Format("{0}: {1} -> {2}", PieceType, from, to);
        }
    }
}

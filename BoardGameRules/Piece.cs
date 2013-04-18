using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Piece
    {
        public string Type { get; private set; }
        public Player Owner { get; private set; }

        public Piece(string type, Player owner)
        {
            this.Type = type;
            this.Owner = owner;
        }

        public override string ToString()
        {
            return Owner.ToString() + ":" + Type;
        }
    }
}

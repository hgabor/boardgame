using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Piece: Context
    {
        public string Type { get; private set; }
        public Player Owner { get; private set; }

        public Piece(string type, Player owner, Game g)
            : base(g)
        {
            this.Type = type;
            this.Owner = owner;
        }

        internal override object GetVariable(string name)
        {
            if (name == "x" || name == "y")
            {
                foreach (var kvp in Game.GetPieces())
                {
                    if (this == kvp.Value)
                    {
                        if (name == "x") return kvp.Key[0];
                        else return kvp.Key[1];
                    }
                }
                return "-1"; // Piece is not on the board
            }
            else if (name == "Owner")
            {
                return Owner;
            }
            else
            {
                return base.GetVariable(name);
            }
        }

        public override string ToString()
        {
            return Owner.ToString() + ":" + Type;
        }
    }
}

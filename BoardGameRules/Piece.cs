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
            if (name == "x" || name == "y" || name == "z")
            {
                Coords c = GetPosition();
                if (c == null) return 0;
                if (name == "x") return c[0];
                else if (name == "y") return c[1];
                else if (name == "z") return c[2];
                else return -1; // WTF?
            }
            else if (name == "Position")
            {
                return GetPosition();
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

        public Coords GetPosition()
        {
            foreach (var kvp in Game.GetPieces())
            {
                if (this == kvp.Value)
                {
                    return kvp.Key;
                }
            }
            return null; // Piece is not on the board
        }

        public override string ToString()
        {
            return Owner.ToString() + ":" + Type;
        }
    }
}

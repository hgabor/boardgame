﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Piece: Context
    {
        public string Type { get; private set; }
        public Player Owner { get; private set; }

        internal Piece(string type, Player owner, GameState g)
            : base(g)
        {
            this.Type = type;
            this.Owner = owner;
        }

        internal override object GetVariable(string name)
        {
            if (name == "x" || name == "y" || name == "z")
            {
                Coords c = GetPosition(GameState.CurrentPlayer);
                if (c == null) return 0;
                if (name == "x") return c[0];
                else if (name == "y") return c[1];
                else if (name == "z") return c[2];
                else return -1; // WTF?
            }
            else if (name == "Position")
            {
                return GetPosition(GameState.CurrentPlayer);
            }
            else if (name == "Owner")
            {
                return Owner;
            }
            else if (name == "Type")
            {
                return Type;
            }
            else
            {
                return base.GetVariable(name);
            }
        }

        public Coords GetPosition()
        {
            return GetPosition(null);
        }

        internal Coords GetPosition(Player asker)
        {
            foreach (var kvp in Game.GetPieces(asker))
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

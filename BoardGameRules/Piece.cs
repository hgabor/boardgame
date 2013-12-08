using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Piece: IReadContext
    {
        public string Type { get; private set; }
        public Player Owner { get; private set; }

        internal Piece(string type, Player owner, Game game)
        {
            this.Type = type;
            this.Owner = owner;
            this.Game = game;
        }

        public object GetVariable(GameState state, string name)
        {
            if (name == "x" || name == "y" || name == "z")
            {
                Coords c = GetPosition(state, state.CurrentPlayer);
                if (c == null) return 0;
                if (name == "x") return c[0];
                else if (name == "y") return c[1];
                else if (name == "z") return c[2];
                else return -1; // WTF?
            }
            else if (name == "Position")
            {
                return GetPosition(state, state.CurrentPlayer);
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
                return null;
            }
        }

        public Coords GetPosition(GameState state)
        {
            return GetPosition(state, null);
        }

        internal Coords GetPosition(GameState state, Player asker)
        {
            foreach (var kvp in Game.GetPieces(state, asker))
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


        public Game Game { get; private set; }
    }
}

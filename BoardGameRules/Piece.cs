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

        internal Piece(string type, Player owner, GameState g)
        {
            this.Type = type;
            this.Owner = owner;
            this.GameState = g;
            this.Game = g.game;
        }

        public object GetVariable(string name)
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
                return null;
            }
        }

        public Coords GetPosition()
        {
            return GetPosition(null);
        }

        internal Coords GetPosition(Player asker)
        {
            foreach (var kvp in Game.GetPieces(GameState, asker))
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
        public GameState GameState { get; private set; }
    }
}

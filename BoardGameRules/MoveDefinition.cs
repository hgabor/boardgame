using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class MoveDefinition: Context
    {
        public readonly string PieceType;
        public readonly string Label;
        public readonly Coords From;
        public readonly Coords To;

        internal MoveDefinition(string pieceType, string label, Coords from, Coords to, Game game)
            : base(game)
        {
            this.PieceType = pieceType;
            this.Label = label;
            this.From = from;
            this.To = to;
        }

        public override string ToString()
        {
            string from = From == null ? "Offboard" : From.ToString();
            string to = To == null ? "Offboard" : To.ToString();
            return string.Format("{0}: {1} -> {2}", PieceType, from, to);
        }

        internal override object GetVariable(string name)
        {
            switch (name)
            {
                case "label":
                    return Label;
                case "from":
                    return Game.Transform(From, Game.CurrentPlayer);
                case "to":
                    return Game.Transform(To, Game.CurrentPlayer);
                default:
                    return base.GetVariable(name);
            }
        }

        public override bool Equals(object obj)
        {
            MoveDefinition d = obj as MoveDefinition;
            if (d == null) return false;
            else return this.Equals(d);
        }

        public bool Equals(MoveDefinition that)
        {
            return this.Label == that.Label &&
                this.PieceType == that.PieceType &&
                object.Equals(this.From, that.From) &&
                object.Equals(this.To, that.To);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 37 + PieceType.GetHashCode();
            if (Label != null) hash = hash * 37 + Label.GetHashCode();
            if (From != null) hash = hash * 37 + From.GetHashCode();
            hash = hash * 37 + To.GetHashCode();
            return hash;
        }
    }
}

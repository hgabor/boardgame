using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public struct GameInt
    {
        public int Value;
        public bool Placeholder;

        public static bool operator == (GameInt i1, GameInt i2) {
            if (i1.Placeholder || i2.Placeholder) return true;
            else return i1.Value == i2.Value;
        }

        public static bool operator !=(GameInt i1, GameInt i2)
        {
            if (i1.Placeholder || i2.Placeholder) return true;
            else return i1.Value != i2.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is GameInt)) return false;
            GameInt gi = (GameInt)obj;
            return Value == gi.Value && Placeholder == gi.Placeholder;
        }
        public bool Equals(GameInt gi)
        {
            return Value == gi.Value && Placeholder == gi.Placeholder;
        }

        public static GameInt Parse(string s)
        {
            if (s == "_") return new GameInt
            {
                Value = 0,
                Placeholder = true
            };
            else return new GameInt
            {
                Value = int.Parse(s),
                Placeholder = false
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (17 * 23 + Value.GetHashCode()) * 23 + Placeholder.GetHashCode();
            }
        }

        public override string ToString()
        {
            if (Placeholder) return "_";
            else return Value.ToString();
        }

        // TODO: implement < <= > >= if needed
    }
}

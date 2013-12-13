using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules
{
    public class Coords: ICallable, IReadContext
    {
        private int[] coords;
        private bool[] placeholder;
        public int Dimension { get { return coords.Length; } }

        public bool IsPlaceHolder
        {
            get
            {
                foreach (bool p in placeholder)
                {
                    if (p) return true;
                }
                return false;
            }
        }

        public bool[] PlaceHolders
        {
            get
            {
                return (bool[])placeholder.Clone();
            }
        }

        public Coords(params int[] coords)
        {
            if (coords.Length == 0) throw new ArgumentException("Coords must have at least one param.");
            this.coords = (int[])coords.Clone();
            this.placeholder = new bool[coords.Length];
        }
        public Coords(int[] coords, bool[] placeholder)
        {
            if (coords.Length == 0) throw new ArgumentException("Coords must have at least one param.");
            if (coords.Length != placeholder.Length) throw new ArgumentException("Coords and placeholder values must have the same length.");
            this.coords = (int[])coords.Clone();
            this.placeholder = (bool[])placeholder.Clone();
        }

        public int this[int d]
        {
            get
            {
                return coords[d];
            }
        }

        internal static Coords Parse(ITree tree)
        {
            int dimension = tree.ChildCount;
            int[] coords = new int[dimension];
            for (int i = 0; i < dimension; i++)
            {
                coords[i] = int.Parse(tree.GetChild(i).Text);
            }
            return new Coords(coords);
        }

        public static bool Match(Coords c1, Coords c2) {
            if (c1.coords.Length != c2.coords.Length) return false;
            for (int i = 0; i < c1.coords.Length; i++)
            {
                if (c1.placeholder[i] || c2.placeholder[i]) continue;
                if (c1.coords[i] != c2.coords[i]) return false;
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(", ", coords));
        }

        public int[] ToInt32Array()
        {
            return Array.ConvertAll(this.coords, gameInt => (int)gameInt);
        }

        public override bool Equals(object obj)
        {
            Coords that = obj as Coords;
            if (that == null) return false;
            if (this.coords.Length != that.coords.Length) return false;
            for (int i = 0; i < coords.Length; i++)
            {
                if (this.coords[i] != that.coords[i]) return false;
                if (this.placeholder[i] != that.placeholder[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = (hash * 17) + coords.Length;
            foreach (int i in coords)
            {
                hash *= 17;
                hash = hash + i.GetHashCode();
            }
            foreach (bool b in placeholder)
            {
                hash *= 17;
                hash = hash + b.GetHashCode();
            }
            return hash;
        }

        object ICallable.Call(GameState state, Context ctx, params object[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Coord needs exactly one param");
            int? i = args[0] as int?;
            if (i == null) throw new ArgumentException("Coord needs integer param");
            if (i <= 0 || i > this.Dimension) throw new ArgumentOutOfRangeException(string.Format("Param must be between 1 and {0}", this.Dimension));
            return this.coords[i.Value - 1];
        }

        object IReadContext.GetVariable(GameState state, string name)
        {
            int dim;
            switch (name)
            {
                case "x": dim = 0; break;
                case "y": dim = 1; break;
                case "z": dim = 2; break;
                default: return null;
            }
            if (dim >= Dimension) return null;
            else return this.coords[dim];
        }

        Game IReadContext.Game
        {
            get { throw new NotImplementedException(); }
        }
    }
}

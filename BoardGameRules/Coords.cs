using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules
{
    public class Coords
    {
        private GameInt[] coords;
        public Coords(params GameInt[] coords)
        {
            this.coords = (GameInt[])coords.Clone();
        }

        public GameInt this[int d]
        {
            get
            {
                return coords[d];
            }
        }

        internal static Coords Parse(ITree tree)
        {
            int dimension = tree.ChildCount;
            GameInt[] coords = new GameInt[dimension];
            for (int i = 0; i < dimension; i++)
            {
                coords[i] = GameInt.Parse(tree.GetChild(i).Text);
            }
            return new Coords(coords);
        }

        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(", ", coords));
        }
    }
}

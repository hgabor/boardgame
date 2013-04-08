using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules
{
    static class Extensions
    {
        public static ITree GetChild(this ITree tree, string index)
        {
            for (int i = 0; i < tree.ChildCount; i++)
            {
                var child = tree.GetChild(i);
                if (child.Text.Equals(index))
                {
                    return child;
                }
            }
            throw new ArgumentException(string.Format("Node {0} doesn't have child {1}!", tree.Text, index));
        }

        public static int GetIntValue(this ITree tree)
        {
            if (tree.ChildCount != 1)
            {
                throw new ArgumentException(string.Format("Node {0} has {1} children, must have exactly 1!", tree.Text, tree.ChildCount));
            }
            return int.Parse(tree.GetChild(0).Text);
        }

        public static Coords GetCoordsValue(this ITree tree)
        {
            if (tree.ChildCount == 0)
            {
                throw new ArgumentException(string.Format("Node {0} has no children, must have at least 1!", tree.Text));
            }
            return Coords.Parse(tree);
        }
    }
}

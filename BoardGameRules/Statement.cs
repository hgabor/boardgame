using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules
{
    abstract class Statement
    {
        public abstract void Run(Context c);

        public static Statement Parse(ITree tree)
        {
            if (tree.Text == "FUNCCALL")
            {
                Expression exp = tree.ParseExpr();
                return new ExprStatement(exp);
            }
            else if (tree.Text == "IF")
            {
                Expression cond = tree.GetChild("IF_CONDITION").GetOnlyChild().ParseExpr();
                ITree actionTree = tree.GetChild("IF_ACTION");
                Statement action = actionTree.ParseStmtList();
                return new IfStatement(cond, action);
            }
            else
            {
                throw new InvalidGameException(tree.FileCoords() + " - Ivalid statement: " + tree.Text);
            }
        }
    }
}

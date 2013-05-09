using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;
using Level14.BoardGameRules.Statements;

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
            else if (tree.Text == "ASSIGNMENT")
            {
                Expression variable = tree.GetChild(0).ParseExpr();
                Expression value = tree.GetChild(1).ParseExpr();
                Statement assignment = new AssignmentStatement(variable, value);
                return assignment;
            }
            else
            {
                throw new InvalidGameException(tree.FileCoords() + " - Ivalid statement: " + tree.Text);
            }
        }
    }
}

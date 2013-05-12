using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;
using Level14.BoardGameRules.Statements;
using Level14.BoardGameRules.Expressions;

namespace Level14.BoardGameRules.Statements
{
    abstract class Statement
    {
        public enum ControlFlow
        {
            Next,
            Return,
        }

        public abstract ControlFlow Run(Context c);

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
            else if (tree.Text == "Return")
            {
                Expression exp = tree.GetOnlyChild().ParseExpr();
                return new ReturnStatement(exp);
            }
            else
            {
                throw new InvalidGameException(tree.FileCoords() + " - Ivalid statement: " + tree.Text);
            }
        }
    }
}

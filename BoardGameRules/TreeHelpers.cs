using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;
using Level14.BoardGameRules.Expressions;
using Level14.BoardGameRules.Statements;
using Antlr.Runtime;

namespace Level14.BoardGameRules
{
    static class TreeHelpers
    {
        public static bool HasChild(this ITree tree, string index)
        {
            for (int i = 0; i < tree.ChildCount; i++)
            {
                var child = tree.GetChild(i);
                if (child.Text.Equals(index))
                {
                    return true;
                }
            }
            return false;
        }

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

        public static ITree GetOnlyChild(this ITree tree)
        {
            if (tree.ChildCount != 1)
            {
                throw new ArgumentException(string.Format("Node {0} has {1} children, must have exactly 1!", tree.Text, tree.ChildCount));
            }
            return tree.GetChild(0);
        }

        public static string FileCoords(this ITree tree)
        {
            return tree.Line.ToString() + ":" + tree.CharPositionInLine.ToString();
        }

        public static Expression ParseExpr(this ITree tree)
        {
            return Expression.Parse(tree);
        }

        public static Statement ParseStmt(this ITree tree)
        {
            return Statement.Parse(tree);
        }

        public static Statement ParseStmtList(this ITree tree)
        {
            var list = new StatementList();
            for (int i = 0; i < tree.ChildCount; i++)
            {
                list.Add(tree.GetChild(i).ParseStmt());
            }
            return list;
        }

        static ITree CreatePlayerNode(int player)
        {
            ITree intTree = new CommonTree(new CommonToken(BoardGameLexer.INT, player.ToString()));
            ITree litInt = new CommonTree(new CommonToken(BoardGameLexer.LIT_INT, "LIT_INT"));
            litInt.AddChild(intTree);
            ITree playerNode = new CommonTree(new CommonToken(BoardGameLexer.PLAYERREF, "PLAYERREF"));
            playerNode.AddChild(litInt);
            return playerNode;
        }

        public delegate bool RewriteRule(ITree input, out ITree output);
        public static void Rewrite(this ITree root, RewriteRule rule)
        {
            List<ITree> newNodes = new List<ITree>();
            for (int i = 0; i < root.ChildCount; i++)
            {
                ITree oldNode = root.GetChild(i);
                if (oldNode.HasChild("Only")) continue;
                ITree newNode;
                bool needed = RewriteNode(oldNode, out newNode, rule);
                if (needed) newNodes.Add(newNode);
            }
            foreach (var n in newNodes)
            {
                root.AddChild(n);
            }
        }

        public static bool RewriteMirrorPlayer(ITree input, out ITree output)
        {
            if (input.Text == "PLAYERREF" && input.GetOnlyChild().Text == "LIT_INT")
            {
                // TODO: make sure null conext is applicable to LIT_INT
                int thisPlayer = (int)input.GetOnlyChild().ParseExpr().Eval(null);
                int otherPlayer = thisPlayer == 1 ? 2 : 1;
                output = CreatePlayerNode(otherPlayer);
                return true;
            }
            output = null;
            return false;
        }

        static bool RewriteNode(ITree root, out ITree output, RewriteRule rule)
        {
            ITree newRoot;
            bool needsRewrite = rule(root, out newRoot);
            if (newRoot == null)
            {
                newRoot = new CommonTree(new CommonToken(root.Type, root.Text));

                for (int i = 0; i < root.ChildCount; i++)
                {
                    ITree newChild;
                    bool branchNeedsRewrite = RewriteNode(root.GetChild(i), out newChild, rule);
                    needsRewrite = needsRewrite || branchNeedsRewrite;
                    newRoot.AddChild(newChild);
                }
            }
            output = newRoot;
            return needsRewrite;
        }
    }
}

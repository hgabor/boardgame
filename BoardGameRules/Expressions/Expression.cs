using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules.Expressions
{
    abstract class Expression
    {
        public abstract object Eval(Context c);

        private static Dictionary<string, ParserFunction> parsers = new Dictionary<string, ParserFunction>();

        protected delegate Expression ParserFunction(ITree tree);
        protected static void RegisterParser(string op, ParserFunction parser)
        {
            if (parsers.ContainsKey(op))
            {
                throw new ArgumentException("Operator " + op + " is already registered!");
            }
            parsers.Add(op, parser);
        }

        protected delegate Expression BinaryConstructor(Expression lhs, Expression rhs);
        protected static void RegisterBinaryParser(string op, BinaryConstructor ctr)
        {
            if (parsers.ContainsKey(op))
            {
                throw new ArgumentException("Operator " + op + " is already registered!");
            }
            parsers.Add(op, tree =>
            {
                Expression lhs = tree.GetChild(0).ParseExpr();
                Expression rhs = tree.GetChild(1).ParseExpr();
                return ctr(lhs, rhs);
            });
        }

        public static Expression Parse(ITree tree)
        {
            string key = tree.Text;
            ParserFunction func;
            if (!parsers.TryGetValue(key, out func))
            {
                throw new InvalidGameException(tree.FileCoords() + " - Unsupported operator: " + key);
            }
            return func(tree);
        }
    }
}

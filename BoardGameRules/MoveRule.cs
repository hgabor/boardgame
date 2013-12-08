using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Expressions;
using Level14.BoardGameRules.Statements;

namespace Level14.BoardGameRules
{
    class MoveRule
    {
        internal string PieceType { get; private set;}

        internal CoordExpr From { get; private set; }
        internal CoordExpr To { get; private set; }

        internal bool TargetMustBeEmpty { get; private set; }
        internal string Label { get; private set; }

        internal Expression Condition { get; private set; }

        private Statement action;
        internal void RunAction(GameState state, Context ctx)
        {
            if (action != null)
            {
                action.Run(state, ctx);
            }
        }

        internal bool OffboardRule { get { return From == null || To == null; } }

        public MoveRule(string piece, CoordExpr from, CoordExpr to, bool targetEmpty, string label, Expression condition, Statement action, Game g)
        {
            PieceType = piece;
            From = from;
            To = to;
            TargetMustBeEmpty = targetEmpty;
            Label = label;
            Condition = condition;
            this.action = action;
        }

        public override string ToString()
        {
            string extraRules = "";
            if (TargetMustBeEmpty) extraRules += " Target must be empty.";
            if (Label != null) extraRules += " #" + Label;
            return string.Format("{0} can move from {1} to {2}.{3}", PieceType, From, To, extraRules);
        }
    }
}

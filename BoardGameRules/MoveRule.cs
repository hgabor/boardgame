﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class MoveRule
    {
        internal string PieceType { get; private set;}

        internal CoordExpr From { get; private set; }
        internal CoordExpr To { get; private set; }

        internal bool TargetMustBeEmpty { get; private set; }

        private Statement action;
        internal void RunAction(Context ctx)
        {
            if (action != null)
            {
                action.Run(ctx);
            }
        }

        internal bool OffboardRule { get { return From == null || To == null; } }

        public MoveRule(string piece, CoordExpr from, CoordExpr to, bool targetEmpty, Statement action)
        {
            PieceType = piece;
            From = from;
            To = to;
            TargetMustBeEmpty = targetEmpty;
            this.action = action;
        }

        public override string ToString()
        {
            string extraRules = "";
            if (TargetMustBeEmpty) extraRules += " Target must be empty.";
            return string.Format("{0} can move from {1} to {2}.{3}", PieceType, From, To, extraRules);
        }
    }
}

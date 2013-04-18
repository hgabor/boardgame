using System;
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

        public MoveRule(string piece, CoordExpr from, CoordExpr to, bool targetEmpty)
        {
            PieceType = piece;
            From = from;
            To = to;
            TargetMustBeEmpty = targetEmpty;
        }

        public override string ToString()
        {
            string extraRules = "";
            if (TargetMustBeEmpty) extraRules += " Target must be empty.";
            return string.Format("{0} can move from {1} to {2}.{3}", PieceType, From, To, extraRules);
        }
    }
}

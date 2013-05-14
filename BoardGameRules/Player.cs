using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Statements;

namespace Level14.BoardGameRules
{
    public class Player
    {
        public int ID { get; private set; }

        internal Player(int id)
        {
            this.ID = id;
        }

        List<Statement> cannotMoveEvents = new List<Statement>();
        internal void AddCannotMove(Statement stmt)
        {
            cannotMoveEvents.Add(stmt);
        }
        internal void CannotMove(Context c)
        {
            foreach (var e in cannotMoveEvents)
            {
                e.Run(c);
            }
        }

        List<Statement> postMoveEvents = new List<Statement>();
        internal void AddPostMove(Statement stmt)
        {
            postMoveEvents.Add(stmt);
        }
        internal void PostMove(Context c)
        {
            foreach (var e in postMoveEvents)
            {
                e.Run(c);
            }
        }

        List<Statement> preMoveEvents = new List<Statement>();
        internal void AddPreMove(Statement stmt)
        {
            preMoveEvents.Add(stmt);
        }
        internal void PreMove(Context c)
        {
            foreach (var e in preMoveEvents)
            {
                e.Run(c);
            }
        }

        public override string ToString()
        {
            return "player" + ID.ToString();
        }

        public bool Won { get; internal set; }
        public bool Tied { get; internal set; }
        public bool Lost { get; internal set; }

        HashSet<Piece> pieces = new HashSet<Piece>();

        internal void AddOffboard(Piece p)
        {
            pieces.Add(p);
        }

        internal bool RemoveOffBoard(Piece p)
        {
            if (pieces.Contains(p))
            {
                pieces.Remove(p);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<Piece> GetOffboard()
        {
            return new HashSet<Piece>(pieces);
        }
    }
}

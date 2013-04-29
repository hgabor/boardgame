using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Player
    {
        public int ID { get; private set; }

        public Player(int id)
        {
            this.ID = id;
        }

        Statement cannotMoveEvent;
        internal void SetCannotMove(Statement stmt)
        {
            if (this.cannotMoveEvent == null) this.cannotMoveEvent = stmt;
            else throw new InvalidGameException("CannotMove event already set for " + this.ToString());
        }
        internal void CannotMove(Context c)
        {
            cannotMoveEvent.Run(c);
        }

        Statement postMoveEvent;
        internal void SetPostMove(Statement stmt)
        {
            if (this.postMoveEvent == null) this.postMoveEvent = stmt;
            else throw new InvalidGameException("FinishedMove event already set for " + this.ToString());
        }
        internal void PostMove(Context c)
        {
            postMoveEvent.Run(c);
        }


        public override string ToString()
        {
            return "player" + ID.ToString();
        }

        public bool Won { get; internal set; }
        public bool Tied { get; internal set; }
        public bool Lost { get; internal set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class GameState: ICloneable
    {
        internal readonly Game game;

        public int CurrentPlayerID { get; set; }
        public Player CurrentPlayer
        {
            get
            {
                return game.GetCurrentPlayer(this);
            }
        }

        internal GameState(Game game)
        {
            this.game = game;
        }

        private GameState(GameState gs)
        {
            this.game = gs.game;
            this.CurrentPlayerID = gs.CurrentPlayerID;
        }

        public GameState Clone()
        {
            return new GameState(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}

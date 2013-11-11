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

        public IEnumerable<MoveDefinition> AllowedMoves { get; set; }

        public Dictionary<Coords, Piece> Board { get; set; }

        internal GameState(Game game)
        {
            this.game = game;
            this.Board = new Dictionary<Coords, Piece>();
        }

        private GameState(GameState gs)
        {
            this.game = gs.game;
            this.CurrentPlayerID = gs.CurrentPlayerID;
            this.Board = new Dictionary<Coords, Piece>(gs.Board);
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

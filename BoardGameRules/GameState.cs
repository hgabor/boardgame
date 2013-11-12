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

        public Context GlobalContext;

        private Dictionary<Player, HashSet<Piece>> offboard = new Dictionary<Player, HashSet<Piece>>();
        public HashSet<Piece> GetOffboard(Player player)
        {
            if (!offboard.ContainsKey(player))
            {
                offboard.Add(player, new HashSet<Piece>());
            }
            return offboard[player];
        }

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
            this.GlobalContext = gs.GlobalContext.Clone(this);
            foreach (var player in game.EnumeratePlayers())
            {
                offboard.Add(player, new HashSet<Piece>(gs.GetOffboard(player)));
            }
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

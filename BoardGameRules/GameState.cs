using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class GameState: ICloneable, IReadContext
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

        internal Player OverrideNextPlayer { get; set; }

        internal Game.GlobalContext GlobalContext;

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
            this.OverrideNextPlayer = gs.OverrideNextPlayer;
            this.GlobalContext = (Game.GlobalContext)gs.GlobalContext.Clone(this);
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

        object IReadContext.GetVariable(GameState state, string name)
        {
            switch (name)
            {
                case "Self":
                    return this;
                default:
                    return null;
            }
        }

        Game IReadContext.Game
        {
            get {
                return this.game;
            }
        }

        public override bool Equals(object obj)
        {
            GameState other = obj as GameState;
            if (other == null) return false;
            else return this.Equals(other);
        }

        public bool Equals(GameState other)
        {
            // States are considered equal, if the same pieces are at the same position, and the current player is the same
            if (CurrentPlayerID != other.CurrentPlayerID) return false;
            if (Board.Count != other.Board.Count) return false;
            foreach (var kvp in Board)
            {
                Piece value;
                if (!other.Board.TryGetValue(kvp.Key, out value)) return false;
                if (!value.Equals(kvp.Value)) return false;
            }
            // TODO: compare offboard pieces
            return true;
        }
    }
}

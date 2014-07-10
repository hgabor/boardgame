using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Level14.BoardGameWeb.Models
{
    /// <summary>
    /// Mapping between a running game and its players (sessions)
    /// </summary>
    public class GameInfo
    {
        public class BasicInfo
        {
            public Guid Id;
            public string GameType;
        }

        public class DetailedInfo
        {
            public Guid Id;
            public string GameType;
            public string CurrentPlayerNick;
            public IEnumerable<PossibleMove> PossibleMoves;
            public bool GameOver;
            public IEnumerable<PieceAt> Pieces;
            public IEnumerable<int> Winners;
        }

        public class PossibleMove
        {
            public int[] From;
            public int[] To;
        }

        public class PieceAt
        {
            public int[] Position;
            public string PieceType;
            public int Player;
        }

        private object syncRoot = new object();

        BoardGameRules.Game game;
        BoardGameRules.GameState state;

        public bool Started
        {
            get
            {
                lock (syncRoot)
                {
                    Debug.Assert(players.Count <= game.PlayerCount);
                    return players.Count == game.PlayerCount;
                }
            }
        }
        public readonly Guid Id = Guid.NewGuid();

        List<Session> players = new List<Session>();
        string gameType;

        public GameInfo(string gameType)
        {
            this.gameType = gameType;
            string rules = System.IO.File.ReadAllText(string.Format("./Games/{0}/{0}.game", gameType));
            game = new BoardGameRules.Game(rules, out state);

            game.SetSelectPieceFunction(SelectPiece);

            Console.WriteLine("New game {0} is ready", gameType);
        }

        public void JoinGame(Session session)
        {
            lock (syncRoot)
            {
                if (Started)
                {
                    throw new GameException("The game has already started.");
                }
                players.Add(session);
                if (players.Count == game.PlayerCount)
                {
                    // Signal the start of the game
                    foreach (var p in players)
                    {
                        p.SignalGameStart(this);
                    }
                }
            }
        }

        public BasicInfo GetBasicInfo()
        {
            lock (syncRoot)
            {
                return new BasicInfo
                {
                    Id = this.Id,
                    GameType = this.gameType
                };
            }
        }

        public DetailedInfo GetDetailedInfo(bool includeMoves)
        {
            lock (syncRoot)
            {
                if (!this.Started)
                {
                    throw new GameException("The game has not been started yet.");
                }

                var info = new DetailedInfo();
                info.Id = this.Id;
                info.CurrentPlayerNick = GetCurrentPlayer().NickName;
                info.GameOver = game.GameOver;
                info.GameType = this.gameType;

                var piecesQuery = this.game.GetPieces(this.state).Select(
                    kvp => new PieceAt {
                        Position = kvp.Key.ToInt32Array(),
                        PieceType = kvp.Value.Type,
                        Player = kvp.Value.Owner.ID
                    });
                // For now, only list the current players offboard pieces
                // TODO: fix it in BoardGameRules
                piecesQuery.Union(this.state.GetOffboard(state.CurrentPlayer).Select(
                    piece => new PieceAt {
                        Position = null,
                        PieceType = piece.Type,
                        Player = state.CurrentPlayerID
                    }));
                info.Pieces = piecesQuery.ToList(); // Enumerate it here before we release the lock
                
                info.Winners = game.Winners.Select(player => player.ID).ToList(); // Enumerate it

                if (includeMoves) {
                    var movesQuery = this.game.EnumeratePossibleMoves(state).Select(
                        move => new PossibleMove {
                            From = move.From != null ? move.From.ToInt32Array() : null,
                            To = move.To.ToInt32Array()
                        });
                    info.PossibleMoves = movesQuery.ToList(); // Enumerate it
                }

                return info;
            }
        }

        private Session GetCurrentPlayer()
        {
            lock (syncRoot)
            {
                int id = game.GetCurrentPlayer(this.state).ID - 1;
                return players[id];
            }
        }

        private BoardGameRules.Piece SelectPiece(BoardGameRules.GameState state, IEnumerable<BoardGameRules.Piece> pieces)
        {
            throw new NotImplementedException();
        }

        // Messaging messages
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Level14.BoardGameWeb.Models
{
    static class Games
    {
        public static GameInfo StartNew(Session gameHost, string gameType)
        {
            if (!Regex.IsMatch(gameType, "[a-zA-Z0-9]+"))
            {
                throw new Exception("Invalid game name");
            }
            var game = new GameInfo(gameType);
            game.JoinGame(gameHost);
            games.Add(game);
            return game;
        }

        public static void Join(Session player, Guid GameId)
        {

        }

        public static void JoinSpectator(Session spectator, Guid GameId)
        {

        }

        private static ConcurrentBag<GameInfo> games = new ConcurrentBag<GameInfo>();
        public static ConcurrentBag<GameInfo> List { get { return games; } }
    }
}

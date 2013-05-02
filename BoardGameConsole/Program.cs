using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Level14.BoardGameRules;

namespace Level14.BoardGameConsole
{
    class Program
    {
        public static void PrintBoard(Game game)
        {
            Console.WriteLine();
            Console.WriteLine("Onboard:");
            foreach (KeyValuePair<Coords, Piece> kvp in game.GetPieces())
            {
                Console.WriteLine("  {0} - {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine("Offboard:");
            Console.WriteLine("  " + string.Join(", ", game.CurrentPlayer.GetOffboard()));
            Console.WriteLine("Current player: {0}", game.CurrentPlayer);
        }

        public static Coords Place(string title, Player p, IEnumerable<Coords> coords) {
            Console.WriteLine("{0}, choose a place for {1}", p, title);
            Console.WriteLine("Places: ");
            int i = 0;
            foreach (Coords c in coords)
            {
                i++;
                Console.Write("{0}  ", c);
                if (i == 4)
                {
                    Console.WriteLine();
                    i = 0;
                }
            }
            while (true)
            {
                string input = Console.ReadLine().ToLower();
                if (input.Length != 2)
                {
                    Console.WriteLine('?');
                    continue;
                }
                int x = input[0] - '0', y = input[1] - '0';
                Coords to = new Coords(x, y);
                if (coords.Any(c => Coords.Match(c, to))) return to;
                Console.WriteLine("Invalid coords!");
            }
        }

        static void Main(string[] args)
        {
            string toParse = @"
Settings (
    PlayerCount: 2;
    BoardDimensions: {8, 8};
    PieceTypes: wolf, dog;
)

StartingBoard (
    Invalid (
       (x + y) % 2 = 0;
    )
    Player(2) (
        dog: {2, 1};
        dog: {4, 1};
        dog: {6, 1};
        dog: {8, 1};
    )
    Player(1) (
        wolf $wolf: Offboard;
    )
)

Moves (
    wolf: Offboard -> Empty {_, _} Then
        NextPlayer(Player(1));
    End;
    wolf: {_, _} -> Empty {x+1, y+1};
    wolf: {_, _} -> Empty {x-1, y+1};
    wolf: {_, _} -> Empty {x+1, y-1};
    wolf: {_, _} -> Empty {x-1, y-1};
    dog: {_, _} -> Empty {x+1, y+1};
    dog: {_, _} -> Empty {x-1, y+1};
)

Events (
    Player(1).CannotMove (
        Win(Player(1));
    )
    Player(2).CannotMove (
        Win(Player(1));
    )
    Player(1).FinishedMove,
    Player(2).FinishedMove (
        If Min([Select y From Pieces Where Owner = Player(2)]) >= $wolf.y Then
              Win(Player(1));
        End
    )
)

";

            try
            {
                Game.SetPlacing(Program.Place);
                Game game = new Game(toParse);

                Console.WriteLine("Game properties: ");
                Console.WriteLine("  Player Count = {0}", game.PlayerCount);
                Console.WriteLine("  Board size = {0}", game.Size);

                // Game loop:
                while (!game.GameOver)
                {
                    PrintBoard(game);
                    string input = Console.ReadLine().ToLower();
                    if (input == "q")
                    {
                        return;
                    }
                    if (input == "moves")
                    {
                        foreach (var move in game.EnumeratePossibleMoves())
                        {
                            Console.WriteLine(move.ToString());
                        }
                        continue;
                    }
                    if (input.Length != 4)
                    {
                        Console.WriteLine('?');
                        continue;
                    }
                    if (input.StartsWith("##"))
                    {
                        int toX = input[2] - '0', toY = input[3] - '0';
                        Coords to = new Coords(toX, toY);
                        // TODO: hardcoded to wolf
                        var pieces = game.CurrentPlayer.GetOffboard();
                        if (pieces.Count() == 0)
                        {
                            Console.WriteLine("No more wolves!");
                            continue;
                        }
                        if (game.TryMakeMoveFromOffboard(pieces.First(), to)) {
                            Console.WriteLine("OK!");
                        }
                        else {
                            Console.WriteLine("Invalid move!");
                        }
                    }
                    else
                    {
                        int fromX = input[0] - '0', fromY = input[1] - '0', toX = input[2] - '0', toY = input[3] - '0';
                        Coords from = new Coords(fromX, fromY), to = new Coords(toX, toY);
                        if (game.TryMakeMove(from, to))
                        {
                            Console.WriteLine("OK!");
                        }
                        else
                        {
                            Console.WriteLine("Invalid move!");
                        }
                    }
                }

                var winners = game.Winners;
                if (winners.Count() == 0)
                {
                    Console.WriteLine("Tie!");
                }
                else
                {
                    Console.WriteLine("Winner(s): {0}", string.Join(", ", winners));
                }
                PrintBoard(game);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

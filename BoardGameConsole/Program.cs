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
            Console.WriteLine("Pieces:");
            foreach (KeyValuePair<Coords, Piece> kvp in game.GetPieces())
            {
                Console.WriteLine("  {0} - {1}", kvp.Key, kvp.Value);
            }
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
    Player(1) (
        dog: {1, 1};
        dog: {3, 1};
        dog: {5, 1};
        dog: {7, 1};
    )
    Player(2) (
        wolf $wolf: [{2,8}, {4,8}, {6,8}, {8,8}];
    )
)

Moves (
    dog: {_, _} -> Empty {x+1, y+1};
    dog: {_, _} -> Empty {x-1, y+1};
    wolf: {_, _} -> Empty {x+1, y+1};
    wolf: {_, _} -> Empty {x-1, y+1};
    wolf: {_, _} -> Empty {x+1, y-1};
    wolf: {_, _} -> Empty {x-1, y-1};
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
        If Min([Select y From Pieces Where Owner = Player(1)]) >= $wolf.y Then
              Win(Player(2));
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
                    if (input.Length != 4)
                    {
                        Console.WriteLine('?');
                        continue;
                    }
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

                var winners = game.Winners;
                if (winners.Count() == 0)
                {
                    Console.WriteLine("Tie!");
                }
                else
                {
                    Console.WriteLine("Winner(s): {0}", string.Join(", ", winners));
                }
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

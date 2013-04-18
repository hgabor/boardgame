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
        wolf: {2, 8};
#        $wolf = wolf: ChoosePosition(Player(2), p => Empty(p));
    )
)

Moves (
# x,y,z -> helyi kontextusban hivatkozás a Target[0,1,2]-re
    dog: {_, _} -> Empty {x+1, y+1};
    dog: {_, _} -> Empty {x-1, y+1};
    wolf: {_, _} -> Empty {x+1, y+1};
    wolf: {_, _} -> Empty {x-1, y+1};
    wolf: {_, _} -> Empty {x+1, y-1};
    wolf: {_, _} -> Empty {x-1, y-1};
)
";

            try
            {
                Game game = new Game(toParse);

                Console.WriteLine("Game properties: ");
                Console.WriteLine("  Player Count = {0}", game.PlayerCount);
                Console.WriteLine("  Board size = {0}", game.Size);

                // Game loop:
                while (true)
                {
                    PrintBoard(game);
                    string input = Console.ReadLine().ToLower();
                    if (input == "q")
                    {
                        break;
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

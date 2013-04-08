using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Level14.BoardGameRules;

namespace Level14.BoardGameConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string toParse = @"
Settings (
    PlayerCount: 2;
    BoardDimensions: {8, 8};
    PieceTypes: wolf, dog;
)

  StartingBoard (
#    Valid (
#        (x + y) % 2 == 0;
#    );
    Player(1) (
        # dog {1, 1};
        # dog {3, 1};
        dog: Source == {1, 1};
        dog: Source == {3, 1};
        dog: Source == {5, 1};
        dog: Source == {7, 1};
    );
    Player(2) (
        $wolf = wolf: ChoosePosition(Player(2), p => Empty(p));
    );
)
";

            try
            {
                Game game = new Game(toParse);

                Console.WriteLine("Game properties: ");
                Console.WriteLine("  Player Count = {0}", game.PlayerCount);
                Console.WriteLine("  Board size = {0}", game.Size);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }
    }
}

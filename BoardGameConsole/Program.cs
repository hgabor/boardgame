using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Level14.BoardGameRules;
using System.Reflection;

namespace Level14.BoardGameConsole
{
    class Program
    {
        public static Coords ParseCoords(string c)
        {
            string[] cString = c.Split(',');
            int[] cInt = new int[cString.Length];
            for (int i = 0; i < cString.Length; i++)
            {
                if (!int.TryParse(cString[i], out cInt[i]))
                {
                    return null;
                }
            }
            return new Coords(cInt);
        }

        public static Piece FindOffboardPiece(Game game, string type)
        {
            foreach (Piece p in game.CurrentPlayer.GetOffboard())
            {
                if (p.Type == type) return p;
            }
            return null;
        }

        public static bool PerformMove(Game game, string fromString, string toString)
        {
            if (fromString[0] == '#' && toString[0] == '#')
            {
                Coords from = ParseCoords(fromString.Substring(1));
                Coords to = ParseCoords(toString.Substring(1));
                if (!game.TryMakeMove(from, to))
                {
                    Console.WriteLine("Invalid move!");
                    return false;
                }
                return true;
            }
            else if (toString[0] == '#')
            {
                Coords to = ParseCoords(toString.Substring(1));
                Piece p = FindOffboardPiece(game, fromString);
                if (p == null)
                {
                    Console.WriteLine("No such piece offboard!");
                    return false;
                }
                else if (!game.TryMakeMoveFromOffboard(p, to)) {
                    Console.WriteLine("Invalid move!");
                    return false;
                }
                return true;
            }
            else {
                Console.WriteLine("Invalid or unsupported move!");
                return false;
            }
        }

        public static void PrintBoard(Game game)
        {
            Console.WriteLine("Board size: {0}", game.Size);
            Console.WriteLine("Player Count: {0}", game.PlayerCount);
            Console.WriteLine("Current player: {0}", game.CurrentPlayer);
            Console.WriteLine();
            Console.WriteLine("Onboard pieces:");
            foreach (KeyValuePair<Coords, Piece> kvp in game.GetPieces())
            {
                Console.WriteLine("  {0} - {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine("Offboard pieces for current player:");
            Console.WriteLine("  " + string.Join(", ", game.CurrentPlayer.GetOffboard()));
        }

        static void PrintHelp()
        {
            Console.WriteLine("Supported commands:");
            Console.WriteLine("?, h             print this help");
            Console.WriteLine("b                print the state of the board");
            Console.WriteLine("l                list possible moves for current player");
            Console.WriteLine("m <from> <to>    move a piece");
            Console.WriteLine("q                quit from the game");
            Console.WriteLine();
            Console.WriteLine("You can enter coordinates in the format: #c1,c2,c3");
            Console.WriteLine("There must be no space characters between the coordinates!");
            Console.WriteLine("If you want to move an offboard piece, write its type instead of coords.");
            Console.WriteLine("Examples:");
            Console.WriteLine("m #3,4 #4,5      move from {3,4} to {4,5}");
            Console.WriteLine("m wolf #4,4      place a 'wolf' from offboard to {4,4}");
        }

        static void PrintMoves(Game game)
        {
            Console.WriteLine("Possible moves:");
            foreach (var rule in game.EnumeratePossibleMoves())
            {
                Console.WriteLine("  " + rule.ToString());
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} gamename", System.Environment.GetCommandLineArgs()[0]);
            Console.WriteLine();
            Console.WriteLine("Supported games are:");
            foreach (var file in System.IO.Directory.EnumerateFiles("Games", "*.game"))
            {
                var gameName = System.IO.Path.GetFileNameWithoutExtension(file);
                Console.WriteLine(gameName);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    PrintUsage();
                    return;
                }

                string fileName = System.IO.Path.Combine("Games", args[0] + ".game");
                if (!System.IO.File.Exists(fileName))
                {
                    Console.WriteLine("{0} does not exists!", fileName);
                    PrintUsage();
                    return;
                }

                Game game = new Game(System.IO.File.ReadAllText(fileName));

                Console.WriteLine("Game loaded successfully");

                PrintHelp();

                while (!game.GameOver)
                {
                    Console.WriteLine();
                    Console.Write("> ");
                    string line = Console.ReadLine().Trim();
                    if (line.Length == 0)
                    {
                        Console.WriteLine("?");
                        continue;
                    }
                    string[] command = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (command[0])
                    {
                        case "?":
                        case "h":
                            PrintHelp();
                            break;
                        case "b":
                            PrintBoard(game);
                            break;
                        case "l":
                            PrintMoves(game);
                            break;
                        case "m":
                            if (command.Length != 3) goto default;
                            if (PerformMove(game, command[1], command[2]))
                            {
                                Console.WriteLine("OK! Next player: {0}", game.CurrentPlayer);
                            }
                            break;
                        case "q":
                            Console.WriteLine("Bye!");
                            return;
                        default:
                            Console.WriteLine("Invalid command! Type ? for help.");
                            break;
                    }
                }

                if (game.PlayerCount == 1)
                {
                    if (game.Winners.Count() == 1)
                    {
                        Console.WriteLine("You won!");
                    }
                    else
                    {
                        Console.WriteLine("You lost!");
                    }
                }
                else
                {
                    if (game.Winners.Count() == 0)
                    {
                        Console.WriteLine("Tie!");
                    }
                    else if (game.Winners.Count() == 1)
                    {
                        Console.WriteLine("The winner is {0}", game.Winners.First());
                    }
                    else
                    {
                        Console.WriteLine("The winners are {0}", string.Join(", ", game.Winners));
                    }
                }
                Console.WriteLine("Press return to quit.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured! Details follow:");
                Console.WriteLine(e);
            }
        }
    }
}

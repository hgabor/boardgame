using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace Level14.BoardGameRules
{
    public class Game
    {
        public int PlayerCount { get; private set; }
        public Coords Size { get; private set; }

        private List<string> pieceTypes = new List<string>();

        public Game(string rules)
        {
            try
            {
                var input = new ANTLRStringStream(rules);
                var lexer = new BoardGameLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new BoardGameParser(tokens);

                var root = parser.parse();

                var errors = parser.GetErrors();

                if (errors.Length > 0)
                {
                    var msg = string.Join("\n", errors);
                    throw new InvalidGameException(string.Format("Number of errors: {0} \n\n{1}", errors.Length, msg));
                }

                // Set player count

                CommonTree t = (CommonTree)root.Tree;

                PlayerCount = t.GetChild("SETTINGS").GetChild("PlayerCount").GetIntValue();
                Size = t.GetChild("SETTINGS").GetChild("BoardDimensions").GetCoordsValue();

                for (int i = 0; i < t.GetChild("SETTINGS").GetChild("PieceTypes").ChildCount; i++)
                {
                    pieceTypes.Add(t.GetChild("SETTINGS").GetChild("PieceTypes").GetChild(i).Text);
                }
            }
            catch (Exception e)
            {
                throw new InvalidGameException(e);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    interface IReadContext
    {
        object GetVariable(GameState state, string name);
        Game Game { get; }
    }
}

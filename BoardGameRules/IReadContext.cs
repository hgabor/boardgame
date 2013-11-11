using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    interface IReadContext
    {
        object GetVariable(string name);
        Game Game { get; }
        GameState GameState { get; }
    }
}

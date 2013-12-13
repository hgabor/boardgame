using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.RegExp
{
    public struct PatternElement
    {


        public Func<GameState, Coords, bool> Predicate;
        public IEnumerable<int> Count;
        public bool IsTarget;
    }
}

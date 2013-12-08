using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class ReadonlyContextWrapper: IWriteContext
    {
        public IReadContext readContext;

        public ReadonlyContextWrapper(IReadContext readContext)
        {
            this.readContext = readContext;
        }

        public void SetVariable(string name, object value)
        {
            throw new InvalidOperationException("Cannot write to a readonly object!");
        }

        public object GetVariable(GameState state, string name)
        {
            return readContext.GetVariable(state, name);
        }

        public Game Game
        {
            get { return readContext.Game; }
        }

        [Obsolete]
        public GameState GameState
        {
            get { return null; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Context: IWriteContext
    {
        Context parent;
        public Game Game { get; private set; }
        public GameState GameState { get; private set; }
        Dictionary<string, object> vars = new Dictionary<string, object>();

        internal Context(GameState gs) {
            this.GameState = gs;
            this.Game = gs.game;
        }

        internal Context(Context parent)
        {
            this.parent = parent;
            this.Game = parent.Game;
            this.GameState = parent.GameState;
        }

        public virtual object GetVariable(string name) {
            object ret;
            if (!vars.TryGetValue(name, out ret))
            {
                if (parent != null) return parent.GetVariable(name);
                else return null;
            }
            return ret;
        }

        public virtual void SetVariable(string name, object value)
        {
            if (parent != null && parent.HasVariable(name))
            {
                parent.SetVariable(name, value);
            }
            else
            {
                vars[name] = value;
            }
        }

        protected virtual bool HasVariable(string name)
        {
            return vars.ContainsKey(name);
        }

        internal Player GetPlayer(int i)
        {
            return Game.GetPlayer(i);
        }

        internal void SetXYZ(Coords cIn, Player asker)
        {
            var c = Game.Transform(cIn, asker);
            if (c.Dimension >= 1)
            {
                SetVariable("x", c[0]);
            }
            if (c.Dimension >= 2)
            {
                SetVariable("y", c[1]);
            }
            if (c.Dimension >= 3)
            {
                SetVariable("z", c[2]);
            }
        }

        internal static Context NewLocal(BoardGameRules.Game game)
        {
            return new Context(game.GetGlobalContext());
        }

    }
}

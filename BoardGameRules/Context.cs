using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Context
    {
        Context parent;
        internal Game Game { get; private set; }
        Dictionary<string, object> vars = new Dictionary<string, object>();

        internal Context(Game g) {
            this.Game = g;
        }

        internal Context(Context parent)
        {
            this.parent = parent;
            this.Game = parent.Game;
        }

        internal virtual object GetVariable(string name) {
            object ret;
            if (!vars.TryGetValue(name, out ret))
            {
                if (parent != null) return parent.GetVariable(name);
                else return null;
            }
            return ret;
        }

        internal void SetVariable(string name, object value)
        {
            if (parent != null && parent.GetVariable(name) != null)
            {
                parent.SetVariable(name, value);
            }
            else
            {
                vars[name] = value;
            }
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

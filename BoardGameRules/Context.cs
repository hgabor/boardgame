using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    internal class Context: IWriteContext
    {
        Context parent;
        public Game Game { get; private set; }
        public GameState GameState { get; private set; }
        Dictionary<string, object> vars = new Dictionary<string, object>();

        /// <summary>
        /// Creates an empty context with no parent.
        /// </summary>
        /// <param name="gs"></param>
        internal Context(GameState gs) {
            this.GameState = gs;
            this.Game = gs.game;
        }

        /// <summary>
        /// Creates am empty context with the specified parent
        /// </summary>
        /// <param name="parent"></param>
        internal Context(Context parent)
        {
            this.parent = parent;
            this.Game = parent.Game;
            this.GameState = parent.GameState;
        }

        /// <summary>
        /// Clones an existing context, except for the associated game state
        /// </summary>
        /// <param name="oldContext"></param>
        /// <param name="newState"></param>
        protected Context(Context oldContext, GameState newState)
        {
            this.parent = oldContext.parent; // FIXME: only works if we don't save any contexts into state
            this.Game = oldContext.Game;
            this.GameState = newState;
            this.vars = new Dictionary<string, object>(oldContext.vars); // TODO: make sure there are only value types
        }

        public virtual object GetVariable(GameState state, string name)
        {
            object ret;
            if (!vars.TryGetValue(name, out ret))
            {
                if (parent != null) return parent.GetVariable(state, name);
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

        internal static Context NewLocal(GameState state)
        {
            return new Context(state.GlobalContext);
        }

        internal virtual Context Clone(BoardGameRules.GameState newState)
        {
            return new Context(this, newState);
        }
    }
}

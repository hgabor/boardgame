using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class MultiParentContext: Context
    {
        Context[] parents;

        public MultiParentContext(GameState g, params Context[] parents)
            : base(g)
        {
            this.parents = (Context[])parents.Clone();
        }

        internal override object GetVariable(string name)
        {
            object ret = base.GetVariable(name);
            if (ret != null) return ret;
            foreach (Context c in parents)
            {
                ret = c.GetVariable(name);
                if (ret != null) return ret;
            }
            return null;
        }
    }
}

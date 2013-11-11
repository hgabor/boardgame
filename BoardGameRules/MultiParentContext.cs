using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class MultiParentContext: Context
    {
        IWriteContext[] parents;

        public MultiParentContext(GameState g, params IReadContext[] parents)
            : base(g)
        {
            this.parents = new IWriteContext[parents.Length];
            for (int i = 0; i < parents.Length; i++)
            {
                var p = parents[i];
                if (p is IWriteContext) this.parents[i] = (IWriteContext)p;
                else { this.parents[i] = new ReadonlyContextWrapper(p); }
            }
        }

        public override object GetVariable(string name)
        {
            object ret = base.GetVariable(name);
            if (ret != null) return ret;
            foreach (var c in parents)
            {
                ret = c.GetVariable(name);
                if (ret != null) return ret;
            }
            return null;
        }
    }
}

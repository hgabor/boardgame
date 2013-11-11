using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class Closure
    {
        private IWriteContext ctx;
        private string name;

        public Closure(IReadContext ctx, string name)
        {
            this.ctx = ctx as IWriteContext;
            if (this.ctx == null) ctx = new ReadonlyContextWrapper(ctx);
            this.name = name;
        }

        public object GetVariable()
        {
            return ctx.GetVariable(name);
        }

        public void SetVariable(object value)
        {
            ctx.SetVariable(name, value);
        }
    }
}

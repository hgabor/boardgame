using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class Closure
    {
        private Context ctx;
        private string name;

        public Closure(Context ctx, string name)
        {
            this.ctx = ctx;
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

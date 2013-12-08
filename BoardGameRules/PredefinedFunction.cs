using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class PredefinedFunction: ICallable
    {
        System.Reflection.MethodInfo method;

        public PredefinedFunction(System.Reflection.MethodInfo method)
        {
            this.method = method;
        }

        public object Call(GameState state, Context ctx, params object[] p)
        {
            var l = new List<object>();
            l.Add(ctx);
            l.AddRange(p);
            return method.Invoke(null, l.ToArray());
        }
    }
}

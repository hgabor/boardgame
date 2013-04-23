using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class Function
    {
        System.Reflection.MethodInfo method;

        public Function(System.Reflection.MethodInfo method)
        {
            this.method = method;
        }

        public object Call(params object[] p)
        {
            return method.Invoke(null, p);
        }
    }
}

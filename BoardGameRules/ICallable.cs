using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    interface ICallable
    {
        object Call(Context ctx, params object[] args);
    }
}

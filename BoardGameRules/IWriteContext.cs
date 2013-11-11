using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    interface IWriteContext: IReadContext
    {
        void SetVariable(string name, object value);
    }
}

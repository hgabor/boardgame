using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class Context
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        public Context() { }

        public object GetVariable(string name) {
            object ret;
            if (!vars.TryGetValue(name, out ret))
            {
                //Try parent...
                return null;
            }
            return ret;
        }

        public void SetVariable(string name, object value)
        {
            vars[name] = value;
        }
    }
}

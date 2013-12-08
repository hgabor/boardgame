using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules.Statements;

namespace Level14.BoardGameRules
{
    class UserFunction: ICallable
    {
        string[] argList;
        Statement body;

        public UserFunction(string[] argList, Statement body)
        {
            this.argList = argList;
            this.body = body;
        }

        object ICallable.Call(GameState state, Context ctx, params object[] args)
        {
            if (argList.Length != args.Length) throw new InvalidGameException("Invalid parameter count!");
            Context local = new Context(ctx);
            for (int i = 0; i < argList.Length; i++)
            {
                local.SetVariable(argList[i], args[i]);
            }
            body.Run(state, local);
            return local.GetVariable(state, "_Return");
        }
    }
}

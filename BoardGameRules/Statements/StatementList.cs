using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Statements
{
    class StatementList: Statement
    {
        List<Statement> stmt = new List<Statement>();

        public override ControlFlow Run(Context c)
        {
            foreach (var s in stmt)
            {
                var flow = s.Run(c);
                if (flow == ControlFlow.Return) return ControlFlow.Return;
            }
            return ControlFlow.Next;
        }

        public void Add(Statement s)
        {
            stmt.Add(s);
        }
    }
}

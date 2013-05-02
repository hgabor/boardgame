using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class StatementList: Statement
    {
        List<Statement> stmt = new List<Statement>();

        public override void Run(Context c)
        {
            foreach (var s in stmt)
            {
                s.Run(c);
            }
        }

        public void Add(Statement s)
        {
            stmt.Add(s);
        }
    }
}

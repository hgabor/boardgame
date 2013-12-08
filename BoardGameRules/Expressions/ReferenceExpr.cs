using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.Expressions
{
    class ReferenceExpr: Expression
    {
        string name;
        public string Name { get { return name; } }

        public ReferenceExpr(string name)
        {
            this.name = name;
        }

        public override object Eval(GameState state, IReadContext c)
        {
            if (c == null)
            {
                // Tried to get member of a null object
                // Return value is null to simplify coding
                return null;
            }
            object o = c.GetVariable(state, name);
            //Allow nulls
            //if (o == null) throw new InvalidGameException(string.Format("Variable {0} does not exist!", name));
            return o;
        }
        
        static ReferenceExpr()
        {
            Expression.RegisterParser("REF", tree =>
            {
                string name = tree.GetOnlyChild().Text;
                return new ReferenceExpr(name);
            });
        }

        public override string ToString()
        {
            return name;
        }
    }
}

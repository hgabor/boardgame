using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr.Runtime;

namespace Level14.BoardGameRules
{
    partial class BoardGameParser
    {
        public AstParserRuleReturnScope<object, IToken> parse()
        {
            return this.sentence();
        }

        List<string> errors = new List<string>();
        public string[] GetErrors()
        {
            return errors.ToArray();
        }
    }
}

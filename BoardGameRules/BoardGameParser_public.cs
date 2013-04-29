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

        public override void ReportError(RecognitionException e)
        {
            if (e is MissingTokenException)
            {
                errors.Add(string.Format("{0}:{1} - Unexpected {2}\n  {3}", e.Line, e.CharPositionInLine, e.Token.Text, e));
            }
            else
            {
                errors.Add(string.Format("{0}:{1} - Error with token {2}\n  {3}", e.Line, e.CharPositionInLine, e.Token.Text, e.ToString()));
            }
            base.ReportError(e);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class InvalidGameException: Exception
    {
        public InvalidGameException(string cause, Exception innerException) : base("Invalid game rules!\n\n" + cause, innerException) { }
        public InvalidGameException(string cause) : this(cause, null) { }
        public InvalidGameException(Exception innerException) : base("Invalid game rules!", innerException) { }
    }
}

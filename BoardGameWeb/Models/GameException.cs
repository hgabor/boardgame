using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameWeb.Models
{
    class GameException : Exception
    {
        public GameException(string p)
            : base(p)
        {
        }
    }
}

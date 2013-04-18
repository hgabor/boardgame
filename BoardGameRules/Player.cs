using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    public class Player
    {
        public int ID { get; private set; }

        public Player(int id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return "player" + ID.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules
{
    class PlaceHolderValue
    {
        private static PlaceHolderValue val = new PlaceHolderValue();
        private PlaceHolderValue() { }
        public static PlaceHolderValue Value { get { return val; } }
    }
}

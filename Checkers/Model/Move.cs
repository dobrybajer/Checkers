using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model
{
    public class Move
    {
        private int from;

        private int to;

        public Move(int moveFrom, int moveTo)
        {
            from = moveFrom;
            to = moveTo;
        }

        public int getFrom()
        {
            return from;
        }

        public int getTo()
        {
            return to;
        }

        public override string ToString()
        {
            return "(" + from + "," + to + ")";
        }
    }
}

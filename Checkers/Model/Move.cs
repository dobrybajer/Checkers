using System.Collections.Generic;

namespace Checkers.Model
{
    public class Move
    {
        private readonly int _from;

        private readonly int _to;

        public List<int> ToRemove { get; set; }

        public Move(int moveFrom, int moveTo)
        {
            _from = moveFrom;
            _to = moveTo;
            ToRemove = new List<int>();
        }

        public int GetFrom()
        {
            return _from;
        }

        public int GetTo()
        {
            return _to;
        }

        public override string ToString()
        {
            return "(" + _from + "," + _to + ")";
        }
    }
}

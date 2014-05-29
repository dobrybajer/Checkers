using System.Windows;

namespace Checkers.Model
{
    struct Info
    {
        public Player Player { get; private set; }
        public PieceType Type { get; private set; }
        public Point Pos { get; private set; }
        public int Index { get; private set; }
        public bool IsSelected { get; private set; }

        public Info(bool value) : this()
        {
            IsSelected = value;
        }

        public void ChangeSelected()
        {
            IsSelected = !IsSelected;
        }

        public void ChangeFields(Player p, PieceType t, Point pos, int i, bool iS)
        {
            Player = p;
            Type = t;
            Pos = pos;
            Index = i;
            IsSelected = iS;
        }
    }
}

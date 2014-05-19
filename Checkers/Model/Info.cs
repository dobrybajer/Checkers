using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Checkers.Model
{
    struct Info
    {
        public PieceType Type;
        public Player Player;
        public Point Pos;
        public int index;
        public bool IsSelected;

        public void ChangeFields(PieceType t, Player p, Point po, int i, bool iS)
        {
            Type = t;
            Player = p;
            Pos = po;
            index = i;
            IsSelected = iS;
        }

        public void SetSelected()
        {
            IsSelected = !IsSelected;
        }

        public bool GetSelected()
        {
            return IsSelected;
        }
    }
}

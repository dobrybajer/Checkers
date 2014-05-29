using System.Windows;
using Checkers.Model;
using GalaSoft.MvvmLight;

namespace Checkers.ViewModel
{
    public class CheckersPiece : ViewModelBase
    {
       
        private Point _pos;
        public Point Pos
        {
            get { return _pos; }
            set { _pos = value; RaisePropertyChanged(() => Pos); }
        }

        private PieceType _type;
        public PieceType Type
        {
            get { return _type; }
            set { _type = value; RaisePropertyChanged(() => Type); }
        }

        private Player _player;
        public Player Player
        {
            get { return _player; }
            set { _player = value; RaisePropertyChanged(() => Player); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged(() => IsSelected); }
        }
    }
}

using System;
using Checkers.Logic;
using Checkers.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Checkers.ViewModel;
using GameLogic = Checkers.Logic.Logic;

namespace Checkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int BoardSize = 8;

        private ObservableCollection<CheckersPiece> _pieces;
        private Info _selected = new Info(false);

        private Player _currentPlayer;
        private readonly GameLogic _logic;
        private readonly Computer _ai;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            CreateBoard();

            _currentPlayer = Player.White;
            _logic = new GameLogic(_pieces, _currentPlayer);
            _ai =  new Computer(_logic);
        }

        private void CreateBoard()
        {
            _pieces = new ObservableCollection<CheckersPiece>();
            for (var i = 0; i < BoardSize; ++i)
            {
                for (var j = 0; j < BoardSize; ++j)
                {
                    if (i < 3 && (i % 2 == 0 && j % 2 == 0 || i % 2 == 1 && j % 2 == 1))
                        _pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.White });
                    else if (i >= 5 && (i % 2 == 0 && j % 2 == 0 || i % 2 == 1 && j % 2 == 1))
                        _pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.Black });
                    else if (i == 3 && j % 2 == 1 || i == 4 && j % 2 == 0)
                        _pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Free, Player = Player.None });
                }
            }

            CheckersBoard.ItemsSource = _pieces;
        }

        private void MakeQueen(int index)
        {
            if (_currentPlayer == Player.White && _pieces[index].Pos.Y == BoardSize - 1 ||
               _currentPlayer == Player.Black && _pieces[index].Pos.Y == 0)
                _pieces[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            _pieces[selected.Index].Player = item2.Player;
            _pieces[selected.Index].Type = item2.Type;
            _pieces[index2].Player = selected.Player;
            _pieces[index2].Type = selected.Type;
        }

        private void ChangePlayer()
        {
            _currentPlayer = _currentPlayer == Player.Black ? Player.White : Player.Black;
            _logic.ChangePlayer();
        }

        private void RemoveDensePawn(Info selected, int index)
        {
            var indexCol = _logic.PosToCol(index);
            var player = _currentPlayer == Player.Black ? -1 : 1;
            var value = selected.Pos.Y % 2 == 0 ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
            var indexx = indexCol < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);
            
            _pieces[selected.Index + player * (value + indexx)].Type = PieceType.Free;
            _pieces[selected.Index + player * (value + indexx)].Player = Player.None; 
        }

        private bool Move(CheckersPiece item, int index)
        {
            var changed = false;

            var good = false;

            var from = _selected.Index;

            var valid = _logic.IsValidMove(from, index);

            if (valid != -1)
            {
                ChangeStates(_selected, item, index);

                if (valid == 1)
                {
                    RemoveDensePawn(_selected, index);

                    if (_logic.CanHit(index))
                    {
                        item.IsSelected = true;
                        _selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
                    }
                    else
                    {
                        changed = true;
                        ChangePlayer();
                        _selected.ChangeSelected();
                        item.IsSelected = false;
                    }
                }

                if (!changed)
                {
                    ChangePlayer();
                    _selected.ChangeSelected();
                    item.IsSelected = false;
                }

                MakeQueen(index);
                good = true;
            }

            if (!good)
            {
                MessageBox.Show("Invalid Move", "Error");
            }
            return good;
        }

        private void MoveEnemy(Move move)
        {
            var info = new Info();
            info.ChangeFields(Player.Black, PieceType.Pawn, 
                new Point(_logic.PosToCol(move.getFrom()), _logic.PosToRow(move.getFrom())), move.getFrom(), false);

            var item = new CheckersPiece
            {
                Player = Player.None, 
                Type = PieceType.Free, 
                Pos = new Point(_logic.PosToCol(move.getTo()), _logic.PosToRow(move.getFrom()))
            };

            ChangeStates(info, item, move.getTo());
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = ((FrameworkElement)sender).DataContext as CheckersPiece;
                var index = _pieces.IndexOf(item);

                if (!_selected.IsSelected)
                {
                    if (item != null && item.Player != _currentPlayer) return;
                    item.IsSelected = true;
                    _selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
                }
                else
                {
                    if (item != null && item.Player == _currentPlayer)
                    {
                        if (_selected.Index == index)
                        {
                            item.IsSelected = false;
                            _selected.ChangeSelected();
                        }
                        else
                        {
                            MessageBox.Show("Wrong Move", "Error");
                        }
                    }
                    else
                    {
                        if (!Move(item, index)) return;

                        var move = _ai.Play();
                        MoveEnemy(move);

                        ChangePlayer();
                        //MessageBox.Show("from " + move.getFrom() + " to " + move.getTo());
                    }
                }
            }
            catch(Exception en)
            {
                MessageBox.Show("error: " + en.Message);
            }
        }
    }
}
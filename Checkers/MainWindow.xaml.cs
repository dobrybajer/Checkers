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

        private readonly GameLogic _logic;
        private readonly Computer _ai;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            CreateBoard();
            
            _logic = new GameLogic(_pieces, Player.White);
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

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = ((FrameworkElement)sender).DataContext as CheckersPiece;
                var index = _pieces.IndexOf(item);

                if (!_logic.Selected.IsSelected)
                {
                    if (item != null && item.Player != _logic.CurrentPlayer) return;
                    item.IsSelected = true;
                    _logic.Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
                }
                else
                {
                    if (item != null && item.Player == _logic.CurrentPlayer)
                    {
                        if (_logic.Selected.Index == index)
                        {
                            item.IsSelected = false;
                            _logic.Selected.ChangeSelected();
                        }
                        else
                        {
                            MessageBox.Show("Wrong Move", "Error");
                        }
                    }
                    else
                    {
                        if (!_logic.MovePlayer(item, index)) return;

                        var move = _ai.Play();

                        if(move != null)
                            _logic.MoveEnemy(move);

                        _logic.ChangePlayer(); 
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
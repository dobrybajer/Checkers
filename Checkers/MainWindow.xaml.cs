using System;
using Checkers.Logic;
using Checkers.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Checkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ObservableCollection<CheckersPiece> Pieces;
        private const int BoardSize = 8;
        private bool IsStarted = false;
        private Info Selected = new Info() { IsSelected = false };
        private Player CurrentPlayer;
        private readonly CheckerMove Logic;
        private Computer ai;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CreateBoard();
            CurrentPlayer = Player.White;
            Logic = new CheckerMove(Pieces, CurrentPlayer);
            ai =  new Computer(Pieces, Player.Black);
        }

        private void CreateBoard()
        {
            Pieces = new ObservableCollection<CheckersPiece>();
            for (int i = 0; i < BoardSize; ++i)
            {
                for (int j = 0; j < BoardSize; ++j)
                {
                    if (i < 3 && (i % 2 == 0 && j % 2 == 0 || i % 2 == 1 && j % 2 == 1))
                        Pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.White });
                    else if (i >= 5 && (i % 2 == 0 && j % 2 == 0 || i % 2 == 1 && j % 2 == 1))
                        Pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.Black });
                    else if (i == 3 && j % 2 == 1 || i == 4 && j % 2 == 0)
                        Pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Free, Player = Player.None });
                }
            }

            CheckersBoard.ItemsSource = Pieces;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IsEnabled = !IsEnabled;
        }

        private void MakeQueen(int index)
        {
            if (CurrentPlayer == Player.White && Pieces[index].Pos.Y == BoardSize - 1 ||
               CurrentPlayer == Player.Black && Pieces[index].Pos.Y == 0)
                Pieces[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            Pieces[selected.index].Player = item2.Player;
            Pieces[selected.index].Type = item2.Type;
            Pieces[index2].Player = selected.Player;
            Pieces[index2].Type = selected.Type;
            //item2.IsSelected = false;
        }

        private void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
            Logic.SetCurrentPlayer(CurrentPlayer);
        }

        private void RemoveDensePawn(Info selected, int index)
        {
            int indexCol = Logic.posToCol(index);
            int player = CurrentPlayer == Player.Black ? -1 : 1;
            int value = selected.Pos.Y % 2 == 0 ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
            int indexx = indexCol < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);
            
            Pieces[selected.index + player * (value + indexx)].Type = PieceType.Free;
            Pieces[selected.index + player * (value + indexx)].Player = Player.None; 
        }

        private void Move(CheckersPiece item, int index)
        {
            bool changed = false;

            bool good = false;

            int from = Selected.index;

            int valid = Logic.IsValidMove(from, index);

            if (valid != -1)
            {
                ChangeStates(Selected, item, index);

                if (valid == 1)
                {
                    RemoveDensePawn(Selected, index);

                    if (Logic.CanHit(index))
                    {
                        item.IsSelected = true;
                        Selected.ChangeFields(item.Type, item.Player, item.Pos, index, true);
                    }
                    else
                    {
                        changed = true;
                        ChangePlayer();
                        Selected.SetSelected();
                        item.IsSelected = false;
                    }
                }

                if (!changed)
                {
                    ChangePlayer();
                    Selected.SetSelected();
                    item.IsSelected = false;
                }

                MakeQueen(index);
                good = true;
            }

            if (!good)
            {
                MessageBox.Show("Invalid Move", "Error");
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = ((FrameworkElement)sender).DataContext as CheckersPiece;
                var index = Pieces.IndexOf(item);
                


                if (!Selected.GetSelected())
                {
                    if (item != null && item.Player != CurrentPlayer) return;
                    item.IsSelected = true;
                    Selected.ChangeFields(item.Type, item.Player, item.Pos, index, true);
                }
                else
                {
                   
                    if (item != null && item.Player == CurrentPlayer)
                    {
                        if (Selected.index == index)
                        {
                            item.IsSelected = false;
                            Selected.SetSelected();
                        }
                        else
                        {
                            MessageBox.Show("Wrong Move", "Error");
                        }
                    }
                    else
                    {
                        Move(item, index);
                        Move move = ai.play();
                        MessageBox.Show("from " + move.getFrom() + " to " + move.getTo());
                        CurrentPlayer = Player.White;

                    }
                }
            }
            catch(Exception en)
            {
                MessageBox.Show("error: " + en.Message);//nasz wyjatek
            }
        }
    }
}
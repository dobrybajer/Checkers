using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Checkers.Model;
using Checkers.Logic;

namespace Checkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<CheckersPiece> Pieces;
        private const int BoardSize = 8;
        private bool IsStarted = false;
        private Info Selected = new Info() { IsSelected = false };
        private Player CurrentPlayer;
        private CheckerMove Logic;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CreateBoard();
            CurrentPlayer = Player.White;
            Logic = new CheckerMove(Pieces, CurrentPlayer);
        }

        private void CreateBoard()
        {
            this.Pieces = new ObservableCollection<CheckersPiece>();
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

            this.CheckersBoard.ItemsSource = Pieces;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IsEnabled = !IsEnabled;
        }

        private void MakeQueen(int index)
        {
            if(CurrentPlayer == Player.White && Pieces[index].Pos.Y == 0 ||
               CurrentPlayer == Player.Black && Pieces[index].Pos.Y == BoardSize - 1)
                Pieces[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            Pieces[selected.index].Player = item2.Player;
            Pieces[selected.index].Type = item2.Type;
            Pieces[index2].Player = selected.Player;
            Pieces[index2].Type = selected.Type;
        }

        private void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
            Logic.SetCurrentPlayer(CurrentPlayer);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext as CheckersPiece;
                var index = Pieces.IndexOf(item);

                if (!Selected.GetSelected())
                {
                    if (item.Player == CurrentPlayer)
                    {
                        item.IsSelected = true;
                        Selected.ChangeFields(item.Type, item.Player, item.Pos, index, true);
                    }
                }
                else
                {
                    if (item.Player == CurrentPlayer)
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
                        bool good = false;

                        int from = Selected.index;
                        if (Logic.isValidMove(from, index))
                        {
                            ChangeStates(Selected, item, index);
                            Selected.SetSelected();
                            item.IsSelected = false;
                            ChangePlayer();
                            MakeQueen(index);

                            //  bool isAttacking = tempBoard.mustAttack ();

                            //  tempBoard.move (from, pos);

                            //  if (isAttacking && tempBoard.mayAttack (pos)) {
                            //    selected.push_back (pos);
                            //    boards.Push (tempBoard);

                            //  }
                            //  else {
                            //    selected.push_back (pos);
                            //    makeMoves (selected, board);
                            //    boards = new Stack ();
                            //  }

                            //  good = true;
                            //}
                            //else if (from == pos) {
                            //  selected.pop_back ();
                            //  boards.Pop ();

                            good = true;
                        }

                        if (!good)
                        {
                            MessageBox.Show("Invalid Move", "Error");
                        }
                    }
                }

            }
            catch
            {
                MessageBox.Show("error");
            }
        }
    }
}

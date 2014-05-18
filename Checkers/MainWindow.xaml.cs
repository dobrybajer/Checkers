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

namespace Checkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<CheckersPiece> Pieces;
        private const int BoardSize = 8;

        public MainWindow()
        {
            InitializeComponent();
            CreateBoard();
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

                    if (i >= 5 && (i % 2 == 0 && j % 2 == 0 || i % 2 == 1 && j % 2 == 1))
                        Pieces.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.Black });
                }

            }

            this.CheckersBoard.ItemsSource = Pieces;
        }
    }
}

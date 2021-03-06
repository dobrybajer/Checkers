﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Checkers.Logic;
using Checkers.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Checkers.ViewModel;
using GameLogic = Checkers.Logic.Logic;
using System.ComponentModel;
using System.Diagnostics;

namespace Checkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private const int BoardSize = 8;
        private const int PawnCount = 12;
        private ObservableCollection<CheckersPiece> _pieces;
        private int _white;
        private int _black;
        private int _whiteHitted;
        private int _blackHitted;

        private Stopwatch _stoper;

        private event EventHandler ComputerTurn;

        private void PlayComputer(object sender, EventArgs e)
        {
            _stoper.Start();
            var move = _ai.Play();

            _logic.MoveEnemy(move);
            _stoper.Stop();

            TimeComputer += _stoper.ElapsedMilliseconds;
            TimeActualComputer = _stoper.ElapsedMilliseconds;
            _stoper.Reset();

            _stoper.Start();
        }

        private double _timeComputer;
        private double _timePlayer;
        private double _timeActualComputer;
        private double _timeActualPlayer;

        public double TimeComputer
        {
            get { return _timeComputer; }
            set
            {  
                _timeComputer = value;
                OnPropertyChanged();
            }
        }

        public double TimePlayer
        {
            get { return _timePlayer; }
            set
            {
                _timePlayer = value;
                OnPropertyChanged();
            }
        }

        public double TimeActualComputer
        {
            get { return _timeActualComputer; }
            set
            {
                _timeActualComputer = value;
                OnPropertyChanged();
            }
        }

        public double TimeActualPlayer
        {
            get { return _timeActualPlayer; }
            set
            {
                _timeActualPlayer = value;
                OnPropertyChanged();
            }
        }

        public int White
        {
            get { return _white; }
            set
            {
                if (value == _white) return;
                _white = value;
                OnPropertyChanged();
            }
        }

        public int Black
        {
            get { return _black; }
            set
            {
                if (value == _black) return;
                _black = value;
                OnPropertyChanged();
            }
        }

        public int WhiteHitted
        {
            get { return _whiteHitted; }
            set
            {
                if (value == _whiteHitted) return;
                _whiteHitted = value;
                OnPropertyChanged();
            }
        }

        public int BlackHitted
        {
            get { return _blackHitted; }
            set
            {
                if (value == _blackHitted) return;
                _blackHitted = value;
                OnPropertyChanged();
            }
        }

        private GameLogic _logic;
        private Computer _ai;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            PrepareGame();
        }

        private void PrepareGame()
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

            _logic = new GameLogic(_pieces, Player.White);
            _ai = new Computer(_logic);

            UpdateInfos();

            _stoper = new Stopwatch();
            _stoper.Start();

            ComputerTurn += PlayComputer;
        }

        private void UpdateInfos()
        {
            White = _pieces.Count(x => x.Player == Player.White);
            Black = _pieces.Count(x => x.Player == Player.Black);
            WhiteHitted = PawnCount - White;
            BlackHitted = PawnCount - Black;

            if (White != 0 && Black != 0)
                return;

            if (White == 0)
                MessageBox.Show("Gracz czarny (komputer) wygrał !");
            if (Black == 0)
                MessageBox.Show("Gratulacje, wygrałeś/aś !");

            PrepareGame();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = ((FrameworkElement)sender).DataContext as CheckersPiece;
                var index = _pieces.IndexOf(item);

                if (!_logic.Selected.IsSelected)
                {
                    if (item == null || item.Player != _logic.CurrentPlayer) return;

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
                        _stoper.Stop();

                        TimePlayer += _stoper.ElapsedMilliseconds;
                        TimeActualPlayer = _stoper.ElapsedMilliseconds;
                        _stoper.Reset();

                        new Task(() => ComputerTurn(this, new EventArgs())).Start();
                    }
                }

                UpdateInfos();
            }
            catch(Exception en)
            {
                MessageBox.Show("error: " + en.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Level_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (combobox == null) return;
            var level = combobox.SelectedIndex;
            if (_ai != null) _ai.SetMaxDepth(level);
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Checkers.Model;
using Checkers.ViewModel;
using System.Windows;

namespace Checkers.Logic
{
    class Logic
    {
        private const int BoardSize = 8;

        private readonly ObservableCollection<CheckersPiece> _pieces;
        public Player CurrentPlayer;
        public Info Selected = new Info(false);

        #region Konstruktor
        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="pieces">Lista kafelków, z których składa się mapa.</param>
        /// <param name="player">Kolor gracza.</param>
        public Logic(ObservableCollection<CheckersPiece> pieces, Player player)
        {
            _pieces = pieces;
            CurrentPlayer = player;
        }
        #endregion

        #region ChangePlayer - GOTOWE
        /// <summary>
        /// Zmienia kolor gracza w logice.
        /// </summary>
        public void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
        }
        #endregion

        #region IsInBoard - GOTOWE
        /// <summary>
        /// Sprawdza czy dane współrzędne znajdują się wewnątrz planszy (między 0 a 8)
        /// </summary>
        /// <param name="x">Wartość x współrzędnej.</param>
        /// <param name="y">Wartość y współrzędnej.</param>
        /// <returns>True jeśli współrzędne są na planszy.</returns>
        private static bool IsInBoard(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }
        #endregion

        // TODO: sprawdźić czy działa, dodać atakowanie w tył
        #region CanHit - poprawić/dokończyć
        private bool CanHit(int position)
        {
            var player = CurrentPlayer == Player.White ? 1 : -1;
            var enemy = CurrentPlayer == Player.White ? Player.Black : Player.White;

            var x = PosToCol(position);
            var y = PosToRow(position);

            var yN = y + player;
            var xNl = x - 1;
            var xNr = x + 1;
            var yF = y + 2 * player;
            var xFl = x - 2;
            var xFr = x + 2;

            return IsInBoard(xNl, yN) && _pieces[ColRowToPos(xNl, yN)].Player == enemy &&
                   IsInBoard(xFl, yF) && _pieces[ColRowToPos(xFl, yF)].Player == Player.None ||
                   IsInBoard(xNr, yN) && _pieces[ColRowToPos(xNr, yN)].Player == enemy &&
                   IsInBoard(xFr, yF) && _pieces[ColRowToPos(xFr, yF)].Player == Player.None;
        }
        #endregion

        // TODO: zrobić ruchy dla królowej, dodać konieczność atakowania
        #region IsValidMove - poprawić/dokończyć
        private int IsValidMove(int from, int to)
        {
            if (from < 0 || from > _pieces.Count - 1 || to < 0 || to > _pieces.Count - 1)
                return -1;

            if (_pieces[from].Type == PieceType.Free || _pieces[to].Type != PieceType.Free)
                return -1;

            if (_pieces[from].Player != CurrentPlayer)
                return -1;

            var color = _pieces[from].Player;

            var enemy = color == Player.White ? Player.Black : Player.White;

            var fromRow = PosToRow(from);
            var fromCol = PosToCol(from);
            var toRow = PosToRow(to);
            var toCol = PosToCol(to);

            int incX, incY;

            if (fromCol > toCol)
                incX = -1;
            else
                incX = 1;

            if (fromRow > toRow)
                incY = -1;
            else
                incY = 1;

            var x = fromCol + incX;
            var y = fromRow + incY;

            if (_pieces[from].Type == PieceType.Pawn)
            {
                int goodDir;

                if ((incY == 1 && color == Player.White) || (incY == -1 && color == Player.Black))
                    goodDir = 0;
                else
                    goodDir = -1;

                if (x == toCol && y == toRow)
                    return goodDir;// && !mustAttack ();

                // If it wasn't a simple move it can only be an attack move
                if (goodDir != -1 && x + incX == toCol && y + incY == toRow && _pieces[ColRowToPos(x, y)].Player == enemy)
                    return 1;
                return -1;
            }
            while (x != toCol && y != toRow && _pieces[ColRowToPos(x, y)].Type == PieceType.Free)
            {
                x += incX;
                y += incY;
            }

            // Simple move with a king piece
            if (x == toCol && y == toRow)
                return -1;// !mustAttack();

            if (_pieces[ColRowToPos(x, y)].Player != enemy) return -1;

            x += incX;
            y += incY;

            while (x != toCol && y != toRow && _pieces[ColRowToPos(x, y)].Type == PieceType.Free)
            {
                x += incX;
                y += incY;
            }

            if (x == toCol && y == toRow)
                return 0;

            return -1;
        }
        #endregion

        #region Konwersja index <-> kolumna, wiersz + sprawdzanie parzystości - GOTOWE
        /// <summary>
        /// Zamienia indeks listy na wartość x (kolumnę) planszy.
        /// </summary>
        /// <param name="value">Indeks listy.</param>
        /// <returns>Wartość x na planszy.</returns>
        private static int PosToCol(int value)
        {
            return (value % 4) * 2 + ((value / 4) % 2 != 0 ? 1 : 0);
        }

        /// <summary>
        /// Zamienia indeks listy na wartość y (wiersz) planszy.
        /// </summary>
        /// <param name="value">Indeks listy.</param>
        /// <returns>Wartość y na planszy.</returns>
        private static int PosToRow(int value)
        {
            return value / 4;
        }

        /// <summary>
        /// Zamienia współrzędne planszy x i y na indeks listy kafelków.
        /// </summary>
        /// <param name="col">Kolumna.</param>
        /// <param name="line">Wiersz.</param>
        /// <returns>Indeks w liście kafelków.</returns>
        private static int ColRowToPos(int col, int line)
        {
            if (IsEven(line))
                return line * 4 + (col + 1) / 2;
            return line * 4 + col / 2;
        }

        /// <summary>
        /// Sprawdza czy wartość jest parzysta. Używane do sprawdzania czy wiersz danego punktu jest parzysty.
        /// </summary>
        /// <param name="value">Wartość do sprawdzenia parzystości.</param>
        /// <returns>True jeśli podana wartość jest parzysta, false w p.p.</returns>
        private static bool IsEven(int value)
        {
            return value % 2 == 0;
        }
        #endregion

        #region ConvertToIntArray - GOTOWE
        /// <summary>
        /// Zamienia listę kafelków na odpowiającą jej tablicę intów uwzględniając kolor i rodzaj pionku na polach.
        /// </summary>
        /// <returns>Tablica intów reprezentująca planszę.</returns>
        public int[] ConvertToIntArray()
        {
            var board = new int[_pieces.Count];
            var i = 0;
            foreach (var e in _pieces)
            {
                if (e.Player == Player.Black && e.Type == PieceType.Pawn)
                    board[i] = -1;
                else if (e.Player == Player.Black && e.Type == PieceType.Queen)
                    board[i] = -2;
                else if (e.Player == Player.White && e.Type == PieceType.Pawn)
                    board[i] = 1;
                else if (e.Player == Player.White && e.Type == PieceType.Queen)
                    board[i] = 2;
                else
                    board[i] = 0;

                ++i;
            }

            return board;
        }
        #endregion

        // TODO: dodać obsługę damek, konieczności bicia, oraz tworzenia Move jako listy indeksów (a nie to i from)
        #region LegalMoves - poprawić/dokończyć
        /// <summary>
        /// Zwraca listę poprawnych ruchów dla całej planszy dla gracza czarnego - komputer.
        /// </summary>
        /// <param name="board">Tablica reprezentująca planszę z pionkami.</param>
        /// <returns>Lista poprawnych ruchów.</returns>
        public List<Move> LegalMoves(int[] board)
        {
            var moves = new List<Move>();

            for (var k = 0; k < _pieces.Count; k++)
                if (board[k] < 0)
                {
                    var x = PosToCol(k);
                    var y = PosToRow(k);
                    int i;

                    if (board[k] == -1)
                    {  // Simple piece
                        i = -1;
                        // See the diagonals /^ e \v
                        if (x < 7 && y + i >= 0 && y + i <= 7 && board[ColRowToPos(x + 1, y + i)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(x + 1, y + i)));
                        }

                        // See the diagonals ^\ e v/
                        if (x > 0 && y + i >= 0 && y + i <= 7 && board[ColRowToPos(x - 1, y + i)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(x - 1, y + i)));
                        }
                    }
                    else
                    { // It's a king piece
                        // See the diagonal \v
                        i = x + 1;
                        var j = y + 1;

                        while (i <= 7 && j <= 7 && board[ColRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(i, j)));

                            i++;
                            j++;
                        }


                        // See the diagonals ^\
                        i = x - 1;
                        j = y - 1;
                        while (i >= 0 && j >= 0 && board[ColRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(i, j)));

                            i--;
                            j--;
                        }

                        // See the diagonals /^
                        i = x + 1;
                        j = y - 1;
                        while (i <= 7 && j >= 0 && board[ColRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(i, j)));

                            i++;
                            j--;
                        }

                        // See the diagonals v/
                        i = x - 1;
                        j = y + 1;
                        while (i >= 0 && j <= 7 && board[ColRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, ColRowToPos(i, j)));

                            i--;
                            j++;
                        }
                    }
                }

            return moves;
        }
        #endregion

        private void MakeQueen(int index)
        {
            if (CurrentPlayer == Player.White && _pieces[index].Pos.Y == BoardSize - 1 ||
               CurrentPlayer == Player.Black && _pieces[index].Pos.Y == 0)
                _pieces[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            _pieces[selected.Index].Player = item2.Player;
            _pieces[selected.Index].Type = item2.Type;
            _pieces[index2].Player = selected.Player;
            _pieces[index2].Type = selected.Type;
        }

        private void RemoveDensePawn(Info selected, CheckersPiece item)
        {
            var indexCol = item.Pos.Y;
            var player = CurrentPlayer == Player.Black ? -1 : 1;
            var value = IsEven((int)selected.Pos.Y) ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
            var indexx = indexCol < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);

            RemovePawn(selected.Index + player * (value + indexx));
        }

        private void RemovePawn(int index)
        {
            _pieces[index].Type = PieceType.Free;
            _pieces[index].Player = Player.None;
        }

        public bool MovePlayer(CheckersPiece item, int index)
        {
            var from = Selected.Index;

            var valid = IsValidMove(from, index);

            if (valid != -1)
            {
                ChangeStates(Selected, item, index);

                if (valid == 1)
                {
                    RemoveDensePawn(Selected, item);

                    if (CanHit(index))
                    {
                        item.IsSelected = true;
                        Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);

                        MakeQueen(index);
                        ChangePlayer();

                        return false;
                    }
                }

                Selected.ChangeSelected();
                item.IsSelected = false;

                MakeQueen(index);
                ChangePlayer();

                return true;
            }

            MessageBox.Show("Invalid Move", "Error");
            return false;
        }

        public void MoveEnemy(Move move)
        {
            var info = new Info();
            info.ChangeFields(Player.Black, PieceType.Pawn,
                new Point(PosToCol(move.GetFrom()), PosToRow(move.GetFrom())), move.GetFrom(), false);

            var item = new CheckersPiece
            {
                Player = Player.None,
                Type = PieceType.Free,
                Pos = new Point(PosToCol(move.GetTo()), PosToRow(move.GetTo()))
            };

            _pieces[move.GetFrom()].IsSelected = false;
            _pieces[move.GetTo()].IsSelected = false;

            foreach (var e in move.ToRemove)
            {
                RemovePawn(e);
            }

            ChangeStates(info, item, move.GetTo());
        }
    }
}

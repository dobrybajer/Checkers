using System.Collections.Generic;
using System.Collections.ObjectModel;
using Checkers.Model;
using Checkers.ViewModel;

namespace Checkers.Logic
{
    class Logic
    {
        private const int BoardSize = 8;

        private readonly ObservableCollection<CheckersPiece> _pieces;
        private Player _currentPlayer;

        #region Konstruktor
        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="pieces">Lista kafelków, z których składa się mapa.</param>
        /// <param name="player">Kolor gracza.</param>
        public Logic(ObservableCollection<CheckersPiece> pieces, Player player)
        {
            _pieces = pieces;
            _currentPlayer = player;
        }
        #endregion

        #region ChangePlayer - GOTOWE
        /// <summary>
        /// Zmienia kolor gracza w logice.
        /// </summary>
        public void ChangePlayer()
        {
            _currentPlayer = _currentPlayer == Player.Black ? Player.White : Player.Black;
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
        public bool CanHit(int position)
        {
            var player = _currentPlayer == Player.White ? 1 : -1;
            var enemy = _currentPlayer == Player.White ? Player.Black : Player.White;

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
        public int IsValidMove(int from, int to)
        {
            if (from < 0 || from > _pieces.Count - 1 || to < 0 || to > _pieces.Count - 1)
                return -1;

            if (_pieces[from].Type == PieceType.Free || _pieces[to].Type != PieceType.Free)
                return -1;

            if (_pieces[from].Player != _currentPlayer)
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
        public int PosToCol(int value)
        {
            return (value % 4) * 2 + ((value / 4) % 2 != 0 ? 1 : 0);
        }

        /// <summary>
        /// Zamienia indeks listy na wartość y (wiersz) planszy.
        /// </summary>
        /// <param name="value">Indeks listy.</param>
        /// <returns>Wartość y na planszy.</returns>
        public int PosToRow(int value)
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
    }
}

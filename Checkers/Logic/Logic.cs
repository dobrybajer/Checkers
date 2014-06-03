using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private void ChangePlayer()
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

        // TODO: dodać sprawdzanie dla obiektów typu Queen
        #region MayAttack - sprawdza tylko dla obiektów typu Pawn
        private bool MayAttack(int position)
        {
            var player = CurrentPlayer == Player.White ? 1 : -1;
            var enemy = CurrentPlayer == Player.White ? Player.Black : Player.White;

            var x = PosToCol(position);
            var y = PosToRow(position);

            // Sprawdzanie bicia w przód
            var yN = y + player;
            var xNl = x - 1;
            var xNr = x + 1;
            var yF = y + 2 * player;
            var xFl = x - 2;
            var xFr = x + 2;

            // Sprawdzanie bicia w tył
            var byN = y - player;
            var bxNl = x - 1;
            var bxNr = x + 1;
            var bYFb = y - 2 * player;
            var bxFlB = x - 2;
            var bxFrB = x + 2;
            if (_pieces[position].Type == PieceType.Pawn)
            {
            return IsInBoard(xNl, yN) && _pieces[ColRowToPos(xNl, yN)].Player == enemy &&
                   IsInBoard(xFl, yF) && _pieces[ColRowToPos(xFl, yF)].Player == Player.None ||
                   IsInBoard(xNr, yN) && _pieces[ColRowToPos(xNr, yN)].Player == enemy &&
                   IsInBoard(xFr, yF) && _pieces[ColRowToPos(xFr, yF)].Player == Player.None ||

                   IsInBoard(bxNl, byN) && _pieces[ColRowToPos(bxNl, byN)].Player == enemy &&
                   IsInBoard(bxFlB, bYFb) && _pieces[ColRowToPos(bxFlB, bYFb)].Player == Player.None ||
                   IsInBoard(bxNr, byN) && _pieces[ColRowToPos(bxNr, byN)].Player == enemy &&
                   IsInBoard(bxFrB, bYFb) && _pieces[ColRowToPos(bxFrB, bYFb)].Player == Player.None;
            }

              var hit = false;
                var nx = x;
                var ny = y;
                 try
            {
                while (IsInBoard(nx + 2, ny + 2))
                {
                    //prawy doł
                    if (_pieces[ColRowToPos(nx + 1, ny + 1)].Player == enemy && _pieces[ColRowToPos(nx + 2, ny + 2)].Player == Player.None)
                        hit = true;
                    else { nx += 1; ny -= 1; }
                }

                nx = x;
                ny = y;
                while (IsInBoard(nx - 2, ny + 2))
                {
                    //lewy doł
                    if (_pieces[ColRowToPos(nx - 1, ny + 1)].Player == enemy && _pieces[ColRowToPos(nx - 2, ny + 2)].Player == Player.None)
                        hit = true;
                    else { nx -= 1; ny += 1; }
                }

                nx = x;
                ny = y;
                while (IsInBoard(nx - 2, ny - 2))
                {
                    //lewy gora
                    if (_pieces[ColRowToPos(nx - 1, ny - 1)].Player == enemy && _pieces[ColRowToPos(nx - 2, ny - 2)].Player == Player.None)
                        hit = true;
                    else { nx -= 1; ny -= 1; }
                }

                nx = x;
                ny = y;
                while (IsInBoard(nx + 2, ny - 2))
                {
                    //prawa gora
                    if (_pieces[ColRowToPos(nx + 1, ny - 1)].Player == enemy && _pieces[ColRowToPos(nx + 2, ny - 2)].Player == Player.None)
                        hit = true;
                    else { nx += 1; ny -= 1; }
                }
            }
            catch (System.Exception e)
            {
                
            }
            return hit;
        }
        #endregion

        #region IsValidMove - GOTOWE
        private int IsValidMove(int from, int to)
        {
            if (from < 0 || from > _pieces.Count - 1 || to < 0 || to > _pieces.Count - 1 || 
                _pieces[from].Type == PieceType.Free || _pieces[to].Type != PieceType.Free ||
                _pieces[from].Player != CurrentPlayer) 
                return -1;

            var color = _pieces[from].Player;

            var enemy = color == Player.White ? Player.Black : Player.White;

            var fromRow = PosToRow(from);
            var fromCol = PosToCol(from);
            var toRow = PosToRow(to);
            var toCol = PosToCol(to);

            var incX = fromCol > toCol ? -1 : 1;

            var incY = fromRow > toRow ? -1 : 1;

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
                    return goodDir + MustAttack();

                // If it wasn't a simple move it can only be an attack move
                if (x + incX == toCol && y + incY == toRow &&
                    _pieces[ColRowToPos(x, y)].Player == enemy)
                    return 1;

                return -1;
            } // else
            while (x != toCol && y != toRow && _pieces[ColRowToPos(x, y)].Type == PieceType.Free)
            {
                x += incX;
                y += incY;
            }

            // Simple move with a king piece
            if (x == toCol && y == toRow)
                return MustAttack();

            if (_pieces[ColRowToPos(x, y)].Player != enemy) return -1;

            x += incX;
            y += incY;

            while (x != toCol && y != toRow && _pieces[ColRowToPos(x, y)].Type == PieceType.Free)
            {
                x += incX;
                y += incY;
            }

            if (x == toCol && y == toRow)
                return 1;

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

        #region MustAttack - GOTOWE
        /// <summary>
        /// Sprawdza czy dany gracz nie ma możliwości ataku.
        /// </summary>
        /// <returns>-1 jeśli dany gracz musi zaatakować jakimś pionkiem.</returns>
        private int MustAttack()
        {
            for (var i = 0; i < 32; i++)
                if (_pieces[i].Player == CurrentPlayer &&
                    (CurrentPlayer == Player.White && _pieces[i].Pos.Y != BoardSize - 2 ||
                    CurrentPlayer == Player.Black && _pieces[i].Pos.Y != 1) &&
                    MayAttack(i))
                    return -1;

            return 0;
        }
        #endregion

        #region MakeQueen - GOTOWE
        /// <summary>
        /// Tworzy damkę ze zwykłego pionka.
        /// </summary>
        /// <param name="index">Indeks obiektu w kolekcji Tile, który ma być zamieniony na damkę.</param>
        private void MakeQueen(int index)
        {
            if (CurrentPlayer == Player.White && _pieces[index].Pos.Y == BoardSize - 1 ||
               CurrentPlayer == Player.Black && _pieces[index].Pos.Y == 0)
                _pieces[index].Type = PieceType.Queen;
        }
        #endregion

        #region ChangeStates - GOTOWE
        /// <summary>
        /// Uaktualnia stan planszy po wykonaniu jednego poprawnego ruchu.
        /// </summary>
        /// <param name="selected">Obeikt zaznaczony do przesunięcia.</param>
        /// <param name="item2">Element planszy, na którym stanie obiekt do przesunięcia.</param>
        /// <param name="index2">Indeks elementu docelowego w kolekcji Tile.</param>
        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            _pieces[selected.Index].Player = item2.Player;
            _pieces[selected.Index].Type = item2.Type;
            _pieces[index2].Player = selected.Player;
            _pieces[index2].Type = selected.Type;
        }
        #endregion

        #region RemoveDensePawn - GOTOWE (uwzględnia zwykły pionek oraz królową dla każdego ruchu)
        /// <summary>
        /// Metoda usuwa z planszy zbity pionek/królową.
        /// </summary>
        /// <param name="selected">Obiekt zaznaczony do ruszenia.</param>
        /// <param name="item">Element planszy, na którym postawimy obiekt 'selected'.</param>
        private void RemoveDensePawn(Info selected, CheckersPiece item)
        {
            if (selected.Type == PieceType.Pawn)
            {
                var player = item.Pos.Y > selected.Pos.Y ? 1 : -1;
                var value = IsEven((int)item.Pos.Y) ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
                var indexx = item.Pos.X < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);

                RemovePawn(selected.Index + player * (value + indexx));
            }
            else
            {
                var x = item.Pos.X;
                var y = item.Pos.Y;
                var vertically = y < selected.Pos.Y ? 1 : -1;
                var horizontally = x < selected.Pos.X ? 1 : -1;

                x += horizontally;
                y += vertically;

                while (_pieces[ColRowToPos((int)x, (int)y)].Player == Player.None)
                {
                    x += horizontally;
                    y += vertically;
                }

                RemovePawn(ColRowToPos((int)x, (int)y));
            }
        }

        /// <summary>
        /// Zmienia dany element w kolekcji Tile na pusty.
        /// </summary>
        /// <param name="index">Indeks elementu w kolekcji Tile do ustawienia na pusty.</param>
        private void RemovePawn(int index)
        {
            _pieces[index].Type = PieceType.Free;
            _pieces[index].Player = Player.None;
        }
        #endregion

        #region MovePlayer - GOTOWE
        /// <summary>
        /// Wykonuje ruch gracza uwzględniając czy jest on poprawny oraz czy jest biciem.
        /// </summary>
        /// <param name="item">Obiekt planszy na który chcemy przenieść zaznaczony pionek.</param>
        /// <param name="index">Indeks obiektu kolekcji Tile.</param>
        /// <returns>True jeśli zakończono daną turę, false jeśli dalej można bić.</returns>
        public bool MovePlayer(CheckersPiece item, int index)
        {
            var from = Selected.Index;

            var valid = IsValidMove(from, index);

            if (valid > -1)
            {
                ChangeStates(Selected, item, index);

                if (valid == 1)
                {
                    RemoveDensePawn(Selected, item);

                    if (MayAttack(index))
                    {
                        item.IsSelected = true;
                        Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);

                        MakeQueen(index);

                        return false;
                    }
                }

                item.IsSelected = false;
                Selected.ChangeSelected();

                MakeQueen(index);
                ChangePlayer();

                return true;
            }

            MessageBox.Show("Invalid Move", "Error");
            return false;
        }
        #endregion

        //------------------------------ Metody tylko dla komputera --------------------------------------------

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

        #region LegalMoves - poprawić/dokończyć
        /// <summary>
        /// Zwraca listę poprawnych ruchów dla całej planszy dla gracza czarnego - komputer.
        /// </summary>
        /// <param name="board">Tablica reprezentująca planszę z pionkami.</param>
        /// <returns>Lista poprawnych ruchów.</returns>
        public List<Move> LegalMoves(int[] board)
        {
            var moves = new List<Move>();
            var attack = ComputerMustAttack(board);

            if (attack != null && attack.Count != 0) // must attack (only piece)
                return attack; 

            for (var k = 0; k < _pieces.Count; k++)
                if (board[k] < 0)
                {
                    var x = PosToCol(k);
                    var y = PosToRow(k);

                    switch (board[k])
                    {
                        case -1: // It's a simple piece
                            if (IsInBoard(x + 1, y - 1) && board[ColRowToPos(x + 1, y - 1)] == 0)
                            {
                                moves.Add(new Move(k, ColRowToPos(x + 1, y - 1)));
                            }
                            if (IsInBoard(x - 1, y - 1) && board[ColRowToPos(x - 1, y - 1)] == 0)
                            {
                                moves.Add(new Move(k, ColRowToPos(x - 1, y - 1)));
                            }
                            break;
                        case -2: // It's a king piece
                            // See the diagonal \v
                            var i = x + 1;
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
                            break;
                    }
                }

            return moves;
        }
        #endregion

        // TODO: dodać uwzględnianie bić wielokrotnych i dla damek
        #region ComputerMayAttack
        private static IEnumerable<Move> ComputerMayAttack(IList<int> board, int i)
        {
            if (board[i] == 0 || board[i] == 1 || board[i] == 2)
                return null;

            var attacks = new List<Move>();

            var x = PosToCol(i);
            var y = PosToRow(i);

            // Sprawdzanie bicia w przód
            var yN = y - 1;
            var xNl = x - 1;
            var xNr = x + 1;
            var yF = y - 2;
            var xFl = x - 2;
            var xFr = x + 2;

            // Sprawdzanie bicia w tył
            var byN = y + 1;
            var bxNl = x - 1;
            var bxNr = x + 1;
            var bYFb = y + 2;
            var bxFlB = x - 2;
            var bxFrB = x + 2;

            if (IsInBoard(xNl, yN) && board[ColRowToPos(xNl, yN)] > 0 && IsInBoard(xFl, yF) &&
                board[ColRowToPos(xFl, yF)] == 0)
                attacks.Add(
                    new Move(i, ColRowToPos(xFl, yF))
                    {
                        ToRemove = new List<int> { ColRowToPos(xNl, yN) }
                    });

            if (IsInBoard(xNr, yN) && board[ColRowToPos(xNr, yN)] > 0 && IsInBoard(xFr, yF) &&
                board[ColRowToPos(xFr, yF)] == 0)
                attacks.Add(
                    new Move(i, ColRowToPos(xFr, yF))
                    {
                        ToRemove = new List<int> { ColRowToPos(xNr, yN) }
                    });

            if (IsInBoard(bxNl, byN) && board[ColRowToPos(bxNl, byN)] > 0 && IsInBoard(bxFlB, bYFb) &&
                board[ColRowToPos(bxFlB, bYFb)] == 0)
                attacks.Add(
                    new Move(i, ColRowToPos(bxFlB, bYFb))
                    {
                        ToRemove = new List<int> { ColRowToPos(bxNl, byN) }
                    });

            if (IsInBoard(bxNr, byN) && board[ColRowToPos(bxNr, byN)] > 0 && IsInBoard(bxFrB, bYFb) &&
                board[ColRowToPos(bxFrB, bYFb)] == 0)
                attacks.Add(
                    new Move(i, ColRowToPos(bxFrB, bYFb))
                    {
                        ToRemove = new List<int> { ColRowToPos(bxNr, byN) }
                    });

            return attacks;
        }
        #endregion

        #region ComputerMustAttack
        private static List<Move> ComputerMustAttack(IList<int> board)
        {
            var attacks = new List<Move>();

            for (var i = 0; i < 32; i++)
            {
                var moves = ComputerMayAttack(board, i);
                if(moves != null && moves.Count() != 0)
                    attacks.AddRange(moves);
            }

            return attacks;
        }
        #endregion

        // TODO: sprawdzić czy działa
        #region MoveEnemy - ?
        public void MoveEnemy(Move move)
        {
            if (move == null)
            {
                MessageBox.Show("No possible moves for computer.");
                return;
            }

          //  MessageBox.Show("from " + move.GetFrom() + " to " + move.GetTo() + " to remove pawns: " + move.ToRemove.Count);

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

            ChangePlayer();
        }
        #endregion
    }
}

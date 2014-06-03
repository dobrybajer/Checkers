using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Logic
{
    class Computer
    {
        /// <summary>
        /// Liczba pól aktywnych na planszy.
        /// </summary>
        private const int Pieces = 32;

        /// <summary>
        /// Logika.
        /// </summary>
        private readonly Logic _logic;

        /// <summary>
        /// Kolor komputera - czarny.
        /// </summary>
        private const int Color = -1;

        /// <summary>
        /// Maksymalna głębokość przeszukiwania drzewa ruchów.
        /// </summary>
        private int _maxDepth = 5;

        /// <summary>
        /// Zmienia maksymalną głębokość przeszukiwania drzewa na wartość value.
        /// </summary>
        /// <param name="value">Nowa wartość maksymalnej głębokości przeszukiwania drzewa.</param>
        public void SetMaxDepth(int value)
        {
            _maxDepth = value + 1;
        }

        /// <summary>
        /// Konstruktor. Tworzy logikę z parametru wejściowego.
        /// </summary>
        /// <param name="l">Instancja klasy logiki.</param>
        public Computer(Logic l)
        {
            _logic = l;
        }

        /// <summary>
        /// Wykonuja ruch w symulacji obliczeń podczas przeszukiwania drzewa ruchów. Uwzględnia bicie pionków.
        /// </summary>
        /// <param name="board">Tablica wraz ze stanem pionków.</param>
        /// <param name="move">Ruch jaki należy wykonać</param>
        private static void Move(ref int[] board, Move move)
        {
            board[move.GetFrom()] = 0;
            board[move.GetTo()] = Color;

            foreach (var e in move.ToRemove)
            {
                board[e] = 0;
            }
        }

        /// <summary>
        /// Metoda zaczyna obliczenia dla komputera. Rozpoczyna przeszukiwanie drzewa (DFS) ruchów.
        /// </summary>
        /// <returns></returns>
        public Move Play()
        {
            Move bestMove = null;
            var board = _logic.ConvertToIntArray();
            var maxValue = Int32.MinValue;
            const int minValue = Int32.MaxValue;
            var sucessors = _logic.LegalMoves(board);

            while (sucessors != null && sucessors.Count() != 0)
            {
                var move = sucessors[0];
                sucessors.RemoveAt(0);

                var nextBoard = (int[])board.Clone();
                
                Move(ref nextBoard, move);

                var value = MinMove(nextBoard, 1, maxValue, minValue);

                if (value > maxValue)
                {
                    maxValue = value;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        #region MaxMove, MinMove
        /// <summary>
        /// Metoda wyznaczająca wartość ruchu z perspektywy gracza.
        /// </summary>
        /// <param name="board">Tablica wraz ze stanem pionków.</param>
        /// <param name="depth">Aktualna głębokość w drzewie przeszukiwań.</param>
        /// <param name="alpha">Aktualne ograniczenie dolne alfa algorytmu alfa-beta.</param>
        /// <param name="beta">Aktualne ograniczenie górne beta algorytmu alfa-beta.</param>
        /// <returns>Ograniczenie dolne alfa lub wartość liścia w drzewie przeszukiwań jeśli osiągnięto maksymalną głębokość przeszukiwania drzewa.</returns>
        private int MaxMove(int[] board, int depth, int alpha, int beta)
        {
            if (CutOffTest(depth))
                return Eval(board);

            var sucessors = _logic.LegalMoves(board);
        
            while (sucessors != null && sucessors.Count != 0)
            {
                var move = sucessors[0];
                sucessors.RemoveAt(0);
      
                var nextBoard = (int[])board.Clone();

                Move(ref nextBoard, move);

                var value = MinMove(nextBoard, depth + 1, alpha, beta);

                if (value > alpha)
                    alpha = value;

                if (alpha > beta)
                    return beta;        
            }

            return alpha;
        }

        /// <summary>
        /// Metoda wyznaczająca wartość ruchu z perspektywy komputera.
        /// </summary>
        /// <param name="board">Tablica wraz ze stanem pionków.</param>
        /// <param name="depth">Aktualna głębokość w drzewie przeszukiwań.</param>
        /// <param name="alpha">Aktualne ograniczenie dolne alfa algorytmu alfa-beta.</param>
        /// <param name="beta">Aktualne ograniczenie górne beta algorytmu alfa-beta.</param>
        /// <returns>Ograniczenie dolne alfa lub wartość liścia w drzewie przeszukiwań jeśli osiągnięto maksymalną głębokość przeszukiwania drzewa.</returns>
        private int MinMove(int[] board, int depth, int alpha, int beta)
        {
            if (CutOffTest(depth))
                return Eval(board);

            var sucessors = _logic.LegalMoves(board);

            while (sucessors != null && sucessors.Count != 0)
            {
                var move = sucessors[0];
                sucessors.RemoveAt(0);

                var nextBoard = (int[])board.Clone();

                Move(ref nextBoard, move);

                var value = MaxMove(nextBoard, depth + 1, alpha, beta);

                if (value < beta)
                    beta = value;

                if (beta < alpha)
                    return alpha;
            }

            return beta;
        }
        #endregion

        #region Eval - wyznacza wartość ruchu komputera
        /// <summary>
        /// Funkcja oblicza wartość ruchu dla komputera. Wykorzystuje kilka różnych funkcji oceniających.
        /// </summary>
        /// <param name="board">Tablica wraz ze stanem pionków.</param>
        /// <returns>Wartość sumy funkcji oceniających.</returns>
        private static int Eval(IList<int> board)
        {
            var colorForce = 0;
            var enemyForce = 0;

            var lboard = board.ToArray();

            const int colorKing = -2;

            for (var i = 0; i < Pieces; i++)
            {
                var piece = board[i];

                if (piece == 0) continue;

                if (piece == Color || piece == colorKing)
                {
                    colorForce += CalculateValueLevel(i, false);
                    colorForce += CalculateValueEdge(i);
                    colorForce += CalculateValuePiece(piece, false);
                    colorForce += CalculateValueHit(i, false, lboard);
                }
                else
                {
                    enemyForce += CalculateValueLevel(i, true);
                    enemyForce += CalculateValueEdge(i);
                    enemyForce += CalculateValuePiece(piece, true);
                    colorForce += CalculateValueHit(i, true, lboard);
                }
            }

            return colorForce - enemyForce;
        }
        #endregion

        #region Funkcje oceniające - poziomu, krawędziowa, rodzaju i ilości pionków
        /// <summary>
        /// Funkcja licząca wartosci dla funkcji poziomu.
        /// </summary>
        /// <param name="pos">Indeks pionka w tablicy pionków.</param>
        /// <param name="white">Informacja czy komputer gra pionkami koloru białego.</param>
        /// <returns>Wartość funkcji oceniającej.</returns>
        private static int CalculateValueLevel(int pos, bool white)
        {
            var value = 0;
            
            if (white) // jesli komputer gra bialymi
            {
                if (pos >= 0 && pos <= 7) // 1 poziom najdalej do lini promujacej
                    value = 1;
                else if (pos >= 8 && pos <= 15) // 2 poziom
                    value = 2;
                else if (pos >= 16 && pos <= 23) // 3 poziom
                    value = 3;
                else if (pos >= 24 && pos <= 31) // 4 poziom najblizej lini promujacej
                    value = 4;
            }
            else // komputer gra czarnymi
            {
                if (pos >= 24 && pos <= 32)//1 poziom najdalej do lini promujacej
                    value = 1;
                else if (pos >= 16 && pos <= 23)//2 poziom
                    value = 2;
                else if (pos >= 8 && pos <= 15)//3 poziom
                    value = 3;
                else if (pos >= 0 && pos <= 7)//4 poziom najblizej lini promujacej
                    value = 4;
            }

            return value;
        }

        /// <summary>
        /// Funkcja licząca wartosci dla funkcji krawędziowej.
        /// </summary>
        /// <param name="pos">Indeks pionka w tablicy pionków.</param>
        /// <returns>Wartość funkcji oceniającej.</returns>
        private static int CalculateValueEdge(int pos)
        {
            int value;

            if ((pos >= 0 && pos <= 3) || pos == 7 || pos == 8 || pos == 15 ||
                pos == 16 || pos == 23 || pos == 24 || (pos >= 28 && pos <= 31)) // obszar 1 czyli skraj planszy
                value = 2;
            else if ((pos >= 4 && pos <= 6) || pos == 11 || pos == 12 ||
                pos == 19 || pos == 20 || (pos >= 25 && pos <= 27)) // obszar 2 obszar oddalony o jeden od kranca planszy
                value = 3;
            else
                value = 4; // obszar 3 srodek planszy

            return value;
        }

        /// <summary>
        /// Funkcja licząca wartosci dla funkcji rodzaj i ilość pionków.
        /// </summary>
        /// <param name="piece">Rodzaj pionka (zawiera informację o kolorze).</param>
        /// <param name="white">Informacja czy komputer gra pionkami koloru białego.</param>
        /// <returns>Wartość funkcji oceniającej.</returns>
        private static int CalculateValuePiece(int piece, bool white)
        {
            var value = 0;

            if (white) //ocena dal pionkow białych
            {
                switch (piece)
                {
                    case 1:
                        value = 10;//zwykly pionek
                        break;
                    case 2:
                        value = 15;//damka
                        break;
                }
            }
            else
            {
                switch (piece)
                {
                    case -1:
                        value = 10;//zwykly pionek
                        break;
                    case -2:
                        value = 15;//damka
                        break;
                }
            }

            return value;
        }

        /// <summary>
        /// Metoda oceniajaca szanse bicia.
        /// </summary>
        /// <param name="pos">rozpatrywanan pozycja</param>
        /// <param name="white">jaki kolor </param>
        /// <param name="board">stan planszy na dana chwile</param>
        /// <returns>Wartosc funkcji oceniajacej</returns>
        private static int CalculateValueHit(int pos, bool white, IList<int> board)
        {
            var value = 0;

            if (white) //bicia dla białych
            {
                if (pos >= 0 && pos <= 23)
                {
                    if (pos % 8 == 0) // pionki pod lewa sciana bez lewego bicia lub pole od
                    {
                        if (board[pos + 4] == -1 || board[pos + 4] == -2) //bicie po prawym skosie
                        {
                            value = board[pos + 9] == 0 ? 8 : 3;
                        }
                        else
                        {
                            value = 2;
                        }

                    }
                    else if ((pos - 4) % 8 == 0)
                    {
                        if (board[pos + 5] == -1 || board[pos + 5] == -2) //bicie po prawym skosie
                        {
                            value = board[pos + 9] == 0 ? 8 : 3;
                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else if ((pos + 1) % 8 == 0) //pionki pod prawa sciana bez prawego bicia
                    {
                        if (board[pos + 4] == -1 || board[pos + 4] == -2) // bicie poo lewym skosie
                        {
                            value = board[pos + 7] == 0 ? 8 : 3;

                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else if ((pos + 5) % 8 == 0)
                    {
                        if (board[pos + 3] == -1 || board[pos + 3] == -2) // bicie poo lewym skosie
                        {
                            value = board[pos + 7] == 0 ? 8 : 3;

                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else
                    {
                        var y = pos / 4;
                        if (y % 2 == 0) //okreslamy w ktorym rzedzie jestesmy
                        {
                            if (board[pos + 3] == -1 || board[pos + 3] == -2) // bicie poo lewym skosie
                            {
                                value = board[pos + 7] == 0 ? 8 : 3;

                            }
                            else if (board[pos + 4] == -1 || board[pos + 4] == -2) //bicie po prawym skosie
                            {
                                value = board[pos + 9] == 0 ? 8 : 3;
                            }
                        }
                        else
                        {
                            if (board[pos + 4] == -1 || board[pos + 4] == -2) // bicie poo lewym skosie
                            {
                                value = board[pos + 7] == 0 ? 8 : 3;
                            }
                            else if (board[pos + 5] == -1 || board[pos + 5] == -2) //bicie po prawym skosie
                            {
                                value = board[pos + 9] == 0 ? 8 : 3;
                            }
                        }

                    }
                }
                else if (pos >= 23 && pos <= 31) //pola bez mozliwosci bicia
                {
                    value = 2;
                }
            }
            else //bicia dla czarnych
            {
                if (pos >= 8 && pos <= 31)
                {
                    if (pos % 8 == 0) // pionki pod lewa sciana bez lewego bicia
                    {
                        if (board[pos - 4] == -1 || board[pos - 4] == -2) //bicie po prawym skosie
                        {
                            value = board[pos - 7] == 0 ? 8 : 3;
                        }
                        else
                        {
                            value = 2;
                        }

                    }
                    else if ((pos - 4) % 8 == 0)
                    {
                        if (board[pos - 3] == -1 || board[pos - 3] == -2) //bicie po prawym skosie
                        {
                            value = board[pos - 7] == 0 ? 8 : 3;
                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else if ((pos + 1) % 8 == 0) //pionki pod prawa sciana bez prawego bicia
                    {
                        if (board[pos - 4] == -1 || board[pos - 4] == -2) // bicie poo lewym skosie
                        {
                            value = board[pos - 9] == 0 ? 8 : 3;

                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else if ((pos + 3) % 8 == 0)
                    {
                        if (board[pos - 5] == -1 || board[pos - 5] == -2) // bicie poo lewym skosie
                        {
                            value = board[pos - 9] == 0 ? 8 : 3;

                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    else
                    {
                        var y = pos / 4;
                        if (y % 2 == 0) //okreslamy w ktorym rzedzie jestesmy
                        {
                            if (board[pos - 5] == -1 || board[pos - 5] == -2) // bicie poo lewym skosie
                            {
                                value = board[pos - 9] == 0 ? 8 : 3;

                            }
                            else if (board[pos - 4] == -1 || board[pos - 4] == -2) //bicie po prawym skosie
                            {
                                value = board[pos - 7] == 0 ? 8 : 3;
                            }
                        }
                        else
                        {
                            if (board[pos - 4] == -1 || board[pos - 4] == -2) // bicie poo lewym skosie
                            {
                                value = board[pos - 9] == 0 ? 8 : 3;

                            }
                            else if (board[pos - 3] == -1 || board[pos - 3] == -2) //bicie po prawym skosie
                            {
                                value = board[pos - 7] == 0 ? 8 : 3;
                            }
                        }

                    }
                }
                else if (pos >= 23 && pos <= 31) //pola bez mozliwosci bicia
                {
                    value = 2;
                }
            }

            return value;
        }
        #endregion

        /// <summary>
        /// Metoda sprawdzająca czy nie osiągneliśmy maksymalnej głębokości przesukiwania drzewa.
        /// </summary>
        /// <param name="depth">Aktualna głębokość przeszukiwania drzewa ruchów.</param>
        /// <returns>True jeśli nie osiągnięto maksymalnej głębokości przeszukiwania.</returns>
        private bool CutOffTest(int depth)
        {
            return depth > _maxDepth;
        }
    }
}

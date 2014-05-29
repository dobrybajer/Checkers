using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Logic
{
    class Computer
    {
        private const int Pieces = 32;
        private readonly Logic _logic;

        private const int Color = -1;

        private const int MaxDepth = 1;

        private readonly int[] _tableWeight = { 4, 4, 4, 4, 
                                                4, 3, 3, 3,
                                                3, 2, 2, 4,
                                                4, 2, 1, 3,
                                                3, 1, 2, 4,
                                                4, 2, 2, 3,
                                                3, 3, 3, 4,
                                                4, 4, 4, 4};

        public Computer(Logic l)
        {
            _logic = l;
        }

        private static void Move(ref int[] board, Move move)
        {
            board[move.getFrom()] = 0;
            board[move.getTo()] = Color;
            
        }
        

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

        private int MaxMove(int[] board, int depth, int alpha, int beta)
        {
            if (CutOffTest(board, depth))
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

        private int MinMove(int[] board, int depth, int alpha, int beta)
        {
            if (CutOffTest(board, depth))
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

        private int Eval(IList<int> board)
        {
            var colorForce = 0;
            var enemyForce = 0;

            const int colorKing = -2;

            for (var i = 0; i < Pieces; i++)
            {
                var piece = board[i];

                if (piece == 0) continue;

                if (piece == Color || piece == colorKing)
                {
                    //zbieramy wyniki dal wszystkich funkcji oceniajacych
                    //colorForce += calculateValue(piece, i);//funkcja oceniajace z bazowego algo
                    colorForce += calculateValueLevel(piece, i,false);//Czwarta funkcja oceniajaca
                    colorForce += calculateValueEdge(piece, i,false);//trzecia funkcja oceniajaca
                    colorForce += CalculateValuePiece(piece, i,false);
                }
                else
                {
                    //enemyForce += calculateValue(piece, i);funkcja oceniajace z bazowego algo
                    enemyForce += calculateValueLevel(piece, i, true);
                    enemyForce += calculateValueEdge(piece, i, true);
                    enemyForce += CalculateValuePiece(piece, i, true);
                }
            }

            return colorForce - enemyForce;
        }

        //Funkcja licząca wartosci dla funkcji poziomu
        private int calculateValueLevel(int piece, int pos,bool white)
        {
            int value = 0;
            
            if (white)//jesli komputer gra bialymi
            {
                if (pos >= 0 && pos <= 7)//pierwszy poziom
                    value = 1;
                else if (pos >= 8 && pos <= 15)//2 poziom
                    value = 2;
                else if (pos >= 16 && pos <= 23)//3 poziom
                    value = 3;
                else if (pos >= 24 && pos <= 31)//4 poziom
                    value = 4;
            }
            else// komputer gra czarnymi
            {
                if (pos >= 24 && pos <= 32)//1 poziom
                    value = 1;
                else if (pos >= 16 && pos <= 23)//2 poziom
                    value = 2;
                else if (pos >= 8 && pos <= 15)//3 poziom
                    value = 3;
                else if (pos >= 0 && pos <= 7)//4 poziom
                    value = 4;

            }

            return value;
        }
        //Funkcja licząca wartosci dla funkcji krawędziowej
        private int calculateValueEdge(int piece, int pos, bool white)
        {
            int value = 0;

            if ((pos >= 0 && pos <= 3) || pos == 7 || pos == 8 || pos == 15 ||
                pos == 16 || pos == 23 || pos == 24 || (pos >= 28 && pos <= 31)) // obszar 1
                value = 2;
            else if ((pos >= 4 && pos <= 6) || pos == 11 || pos == 12 ||
                pos == 19 || pos == 20 || (pos >= 25 && pos <= 27)) // obszar 2
                value = 3;
            else
                value = 4;//obszar 3

            return value;
        }
        // FUnkcja liczaca wartosci dla funkcji rodzaj i ilosc pionkow
        private int CalculateValuePiece(int piece, int pos, bool white)
        {
            int value = 0;
            
            if (white)
            {
                if (piece == 1)
                    value = 1;
                else if (piece == 2)
                    value = 5;
            }
            else
            {
                if (piece == -1)
                    value = 1;
                else if (piece == -2)
                    value = 5;
            }

            return value;
        }

        private int calculateValueHit(int piece, int pos, bool white)
        {
            int value = 0;


            return value;
        }



        private int CalculateValue(int piece, int pos)
        {
            int value;

            if (piece == 1) //Simple piece
                if (pos >= 4 && pos <= 7)
                    value = 7;
                else
                    value = 5;
            else if (piece != -1) //Simple piece
                if (pos >= 24 && pos <= 27)
                    value = 7;
                else
                    value = 5;
            else // king piece
                value = 10;

            return value * _tableWeight[pos];
        }

        private static bool CutOffTest(ICollection<int> board, int depth)
        {
            return depth > MaxDepth || board.Count == 0;
        }
    }
}

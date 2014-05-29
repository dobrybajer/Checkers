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
                    colorForce += CalculateValue(piece, i);
                else
                    enemyForce += CalculateValue(piece, i);
            }

            return colorForce - enemyForce;
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

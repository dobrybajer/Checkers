using System.Windows;
using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkers.Logic;
using System.Diagnostics;

namespace Checkers.Logic
{
    class Computer
    {
        // Board used for the computer moves
        private ObservableCollection<CheckersPiece> Pieces;
        private Player CurrentPlayer;
        private readonly CheckerMove logic;

        // Computer's pieces color
        private int color;

        // Max depth used in the Min-Max algorithm
        private int maxDepth = 1;

        // Weights used for the board
        private int[] tableWeight = { 4, 4, 4, 4, 
                                 4, 3, 3, 3,
                                 3, 2, 2, 4,
                                 4, 2, 1, 3,
                                 3, 1, 2, 4,
                                 4, 2, 2, 3,
                                 3, 3, 3, 4,
                                 4, 4, 4, 4};



        /**
         * Constructor.
         * @param gameBoard Tabuleiro que o computador deve usar para efectuar as jogadas.
         */
        public Computer(ObservableCollection<CheckersPiece> pieces, Player player)
        {
            Pieces = pieces;
            CurrentPlayer = player;
            logic = new CheckerMove(Pieces, CurrentPlayer);
        }

        byte[,] Convert()
        {
            var board = new byte[8,8];
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; j++)
                {
                    
                }    
            }
        }

        /// <sumary> 
        ///   Allows the user to change the max depth of
        ///  the min-max tree
        /// </sumary>
        public int depth
        {
            get
            {
                return maxDepth;
            }
            set
            {
                maxDepth = value;
            }
        }

        private void MakeQueen(int index)
        {
            int BoardSize = 8;
            if (CurrentPlayer == Player.White && Pieces[index].Pos.Y == BoardSize - 1 ||
               CurrentPlayer == Player.Black && Pieces[index].Pos.Y == 0)
                Pieces[index].Type = PieceType.Queen;
        }

        private void ChangeStates(int index1, int index2)
        {
            CheckersPiece selected = new CheckersPiece() {Player = Pieces[index1].Player, Type = Pieces[index1].Type};
            CheckersPiece item2 = new CheckersPiece() {Player = Pieces[index2].Player, Type = Pieces[index2].Type};

            Pieces[index1].Player = item2.Player;
            Pieces[index1].Type = item2.Type;
            Pieces[index2].Player = selected.Player;
            Pieces[index2].Type = selected.Type;
        }

        private void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
            logic.SetCurrentPlayer(CurrentPlayer);
        }

        private void RemoveDensePawn(int index1, int index2)
        {
            CheckersPiece selected = new CheckersPiece() { Player = Pieces[index1].Player, Type = Pieces[index1].Type };

            int indexCol = logic.posToCol(index2);
            int player = CurrentPlayer == Player.Black ? -1 : 1;
            int value = selected.Pos.Y % 2 == 0 ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
            int indexx = indexCol < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);

            Pieces[index1 + player * (value + indexx)].Type = PieceType.Free;
            Pieces[index1 + player * (value + indexx)].Player = Player.None;
        }

        private void Move(int from, int to)
        {
            ChangeStates(from, to);

            if (Math.Abs(from - to) > 4)
                RemoveDensePawn(from, to);

            ChangePlayer();
          
            MakeQueen(to);   
        }

        /// <sumary> 
        ///   Makes the computer play a move in the checkers
        ///  board that it holds.
        /// </sumary>
        public void play()
        {
            try
            {
                Move move = minimax();

                if (move != null)
                    Move(move.getFrom(), move.getTo());
            }
            catch (Exception e)
            {
                MessageBox.Show("error: " + e.Message);
            }
        }

        /// <sumary> 
        ///   Says if the game move is valid
        /// </sumary>
        /// <param name="moves">
        ///  The list of piece movements for the game move.
        /// </param>
        /// <value>
        ///  true if the game move is valid, false otherwise.
        /// </value>
        private bool mayPlay(List<Move> moves)
        {
            return moves.Count != 0;
        }


        /// <sumary> 
        ///   Implements the Min-Max algorithm for selecting
        ///  the computer move
        /// </sumary>
        /// <param name="board">
        ///   The board that will be used as a starting point
        ///  for generating the game movements
        /// </param>
        /// <value>
        ///  A list with the computer game movements.
        /// </value>
        private Move minimax()
        {
            List<Move> sucessors;
            Move move, bestMove = null;
            CheckersPiece[] nextBoard;
            int value, maxValue = Int32.MinValue;

            sucessors = logic.LegalMoves();
            while (mayPlay(sucessors))
            {
                move = sucessors[0];
                var tmp = Pieces.ToArray();
                nextBoard = (CheckersPiece[])tmp.Clone();

                Debug.WriteLine("******************************************************************");
                nextBoard.move(move);
                value = minMove(nextBoard, 1, maxValue, Int32.MaxValue);

                if (value > maxValue)
                {
                    Debug.WriteLine("Max value : " + value + " at depth : 0");
                    maxValue = value;
                    bestMove = move;
                }
            }

            Debug.WriteLine("Move value selected : " + maxValue + " at depth : 0");

            return bestMove;
        }

        /// <sumary> 
        ///   Implements game move evaluation from the point of view of the
        ///  MAX player.
        /// </sumary>
        /// <param name="board">
        ///   The board that will be used as a starting point
        ///  for generating the game movements
        /// </param>
        /// <param name="depth">
        ///   Current depth in the Min-Max tree
        /// </param>
        /// <param name="alpha">
        ///   Current alpha value for the alpha-beta cutoff
        /// </param>
        /// <param name="beta">
        ///   Current beta value for the alpha-beta cutoff
        /// </param>
        /// <value>
        ///  Move evaluation value
        /// </value>
        private int maxMove(ObservableCollection<CheckersPiece> board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);


            List sucessors;
            List move;
            ObservableCollection<CheckersPiece> nextBoard;
            int value;

            Debug.WriteLine("Max node at depth : " + depth + " with alpha : " + alpha +
                                " beta : " + beta);

            sucessors = board.legalMoves();
            while (mayPlay(sucessors))
            {
                move = (List)sucessors.pop_front();
                nextBoard = (ObservableCollection<CheckersPiece>)board.clone();
                nextBoard.move(move);
                value = minMove(nextBoard, depth + 1, alpha, beta);

                if (value > alpha)
                {
                    alpha = value;
                    Debug.WriteLine("Max value : " + value + " at depth : " + depth);
                }

                if (alpha > beta)
                {
                    Debug.WriteLine("Max value with prunning : " + beta + " at depth : " + depth);
                    Debug.WriteLine(sucessors.length() + " sucessors left");
                    return beta;
                }

            }

            Debug.WriteLine("Max value selected : " + alpha + " at depth : " + depth);
            return alpha;
        }

        /// <sumary> 
        ///   Implements game move evaluation from the point of view of the
        ///  MIN player.
        /// </sumary>
        /// <param name="board">
        ///   The board that will be used as a starting point
        ///  for generating the game movements
        /// </param>
        /// <param name="depth">
        ///   Current depth in the Min-Max tree
        /// </param>
        /// <param name="alpha">
        ///   Current alpha value for the alpha-beta cutoff
        /// </param>
        /// <param name="beta">
        ///   Current beta value for the alpha-beta cutoff
        /// </param>
        /// <value>
        ///  Move evaluation value
        /// </value>
        private int minMove(ObservableCollection<CheckersPiece> board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);


            List sucessors;
            List move;
            ObservableCollection<CheckersPiece> nextBoard;
            int value;

            Debug.WriteLine("Min node at depth : " + depth + " with alpha : " + alpha +
                                " beta : " + beta);

            sucessors = (List)board.legalMoves();
            while (mayPlay(sucessors))
            {
                move = (List)sucessors.pop_front();
                nextBoard = (ObservableCollection<CheckersPiece>)board.clone();
                nextBoard.move(move);
                value = maxMove(nextBoard, depth + 1, alpha, beta);

                if (value < beta)
                {
                    beta = value;
                    Debug.WriteLine("Min value : " + value + " at depth : " + depth);
                }

                if (beta < alpha)
                {
                    Debug.WriteLine("Min value with prunning : " + alpha + " at depth : " + depth);
                    Debug.WriteLine(sucessors.length() + " sucessors left");
                    return alpha;
                }
            }

            Debug.WriteLine("Min value selected : " + beta + " at depth : " + depth);
            return beta;
        }

        /// <sumary> 
        ///   Evaluates the strength of the current player
        /// </sumary>
        /// <param name="board">
        ///   The board where the current player position will be evaluated.
        /// </param>
        /// <value>
        ///  Player strength
        /// </value>
        private int eval(ObservableCollection<CheckersPiece> board)
        {
            int colorKing;
            int colorForce = 0;
            int enemyForce = 0;
            int piece;

            if (color == ObservableCollection<CheckersPiece>.WHITE)
                colorKing = ObservableCollection<CheckersPiece>.WHITE_KING;
            else
                colorKing = ObservableCollection<CheckersPiece>.BLACK_KING;

            try
            {
                for (int i = 0; i < 32; i++)
                {
                    piece = board.getPiece(i);

                    if (piece != ObservableCollection<CheckersPiece>.EMPTY)
                        if (piece == color || piece == colorKing)
                            colorForce += calculateValue(piece, i);
                        else
                            enemyForce += calculateValue(piece, i);
                }
            }
            catch (BadCoord bad)
            {
                Debug.WriteLine(bad.StackTrace);
                Application.Exit();
            }

            return colorForce - enemyForce;
        }

        /// <sumary> 
        ///   Evaluates the strength of a piece
        /// </sumary>
        /// <param name="piece">
        ///   The type of piece
        /// </param>
        /// <param name="pos">
        ///   The piece position
        /// </param>
        /// <value>
        ///  Piece value
        /// </value>
        private int calculateValue(int piece, int pos)
        {
            int value;

            if (piece == ObservableCollection<CheckersPiece>.WHITE) //Simple piece
                if (pos >= 4 && pos <= 7)
                    value = 7;
                else
                    value = 5;
            else if (piece != ObservableCollection<CheckersPiece>.BLACK) //Simple piece
                if (pos >= 24 && pos <= 27)
                    value = 7;
                else
                    value = 5;
            else // king piece
                value = 10;

            return value * tableWeight[pos];
        }


        /// <sumary> 
        ///   Verifies if the game tree can be prunned
        /// </sumary>
        /// <param name="board">
        ///   The board to evaluate
        /// </param>
        /// <param name="depth">
        ///   Current game tree depth
        /// </param>
        /// <value>
        ///  true if the tree can be prunned.
        /// </value>
        private bool cutOffTest(ObservableCollection<CheckersPiece> board, int depth)
        {
            return depth > maxDepth || board.hasEnded();
        }
    }
}

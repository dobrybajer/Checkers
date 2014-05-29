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

        int[] ConvertToIntArray()
        {
            var board = new int[Pieces.Count];
            int i = 0;
            foreach (var e in Pieces)
            {
                switch (e.Player)
                {
                    case Player.Black:
                        board[i] = -1;
                        break;
                    case Player.White:
                        board[i] = 1;
                        break;
                    default:
                        board[i] = 0;
                        break;
                }
                ++i;
            }

            return board;
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

      

        private void Move(ref int[] board, Move move)
        {
            int from = move.getFrom();
            int to = move.getTo();

            board[from] = 0;
            board[to] = -1;

        }

        /// <sumary> 
        ///   Makes the computer play a move in the checkers
        ///  board that it holds.
        /// </sumary>
        public Move play()
        {
            return minimax();
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
            int[] Board = ConvertToIntArray();
            int[] nextBoard;
            int value, maxValue = Int32.MinValue;


            sucessors = LegalMoves(Board);
            try
            {
                while (mayPlay(sucessors))
                {
                    move = sucessors[0];
                    sucessors.RemoveAt(0);

                    nextBoard = (int[])Board.Clone();

                    Debug.WriteLine("******************************************************************");
                    Move(ref nextBoard, move);
                    value = minMove(nextBoard, 1, maxValue, Int32.MaxValue);

                    if (value > maxValue)
                    {
                        Debug.WriteLine("Max value : " + value + " at depth : 0");
                        maxValue = value;
                        bestMove = move;
                    }

          
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("error in minmax: " + e.Message);
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
        private int maxMove(int[] board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);


            List<Move> sucessors;
            Move move;
            int[] nextBoard;
            int value;

            Debug.WriteLine("Max node at depth : " + depth + " with alpha : " + alpha +
                                " beta : " + beta);

            sucessors = LegalMoves(board);
        
            while (mayPlay(sucessors))
            {
                move = sucessors[0];
                sucessors.RemoveAt(0);
      
                nextBoard = (int[])board.Clone();
                Move(ref nextBoard, move);
                value = minMove(nextBoard, depth + 1, alpha, beta);

                if (value > alpha)
                {
                    alpha = value;
                    Debug.WriteLine("Max value : " + value + " at depth : " + depth);
                }

                if (alpha > beta)
                {
                    Debug.WriteLine("Max value with prunning : " + beta + " at depth : " + depth);
                    Debug.WriteLine(sucessors.Count + " sucessors left");
                    return beta;
                }

            }

            Debug.WriteLine("Max value selected : " + alpha + " at depth : " + depth);
            return alpha;
        }


        public List<Move> LegalMoves(int[] board)
        {
            Player color, enemy;

            color = CurrentPlayer;
            if (color == Player.White)
                enemy = Player.Black;
            else
                enemy = Player.Black;

            return generateMoves(color, enemy, board);
        }

        public int posToCol(int value)
        {
            return (value % 4) * 2 + ((value / 4) % 2 != 0 ? 1 : 0);
        }

        public int posToRow(int value)
        {
            return value / 4;
        }

        private bool isEven(int value)
        {
            return value % 2 == 0;
        }

        public int colRowToPos(int col, int line)
        {
            if (isEven(line))
                return line * 4 + (col + 1) / 2;
            else
                return line * 4 + col / 2;
        }

        private List<Move> generateMoves(Player color, Player enemy, int[] board)
        {
            List<Move> moves = new List<Move>();
            var white = false;
            if (CurrentPlayer == Player.White)
                white = true;

            for (int k = 0; k < Pieces.Count; k++)
                if (white ? board[k] >0: board[k] <0)
                {
                    int x = posToCol(k);
                    int y = posToRow(k);
                    int i, j;

                    if (white ? board[k] == 1 : board[k] == -1)
                    {  // Simple piece
                        i = (white) ? 1 : -1;
                        // See the diagonals /^ e \v
                        if (x < 7 && y + i >= 0 && y + i <= 7 &&  board[colRowToPos(x + 1, y + i)] ==0)
                        {
                            moves.Add(new Move(k, colRowToPos(x + 1, y + i)));

                        }

                        // See the diagonals ^\ e v/
                        if (x-1 >= 0 && y + i >= 0 && y + i <= 7 &&
                            board[colRowToPos(x - 1, y + i)] == 0)
                        {
                            moves.Add(new Move(k, colRowToPos(x - 1, y + i)));
                        };
                    }
                    else
                    { // It's a king piece
                        // See the diagonal \v
                        i = x + 1;
                        j = y + 1;

                        while (i <= 7 && j <= 7 && board[colRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i++;
                            j++;
                        }


                        // See the diagonals ^\
                        i = x - 1;
                        j = y - 1;
                        while (i >= 0 && j >= 0 && board[colRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i--;
                            j--;
                        }

                        // See the diagonals /^
                        i = x + 1;
                        j = y - 1;
                        while (i <= 7 && j >= 0 && board[colRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i++;
                            j--;
                        }

                        // See the diagonals v/
                        i = x - 1;
                        j = y + 1;
                        while (i >= 0 && j <= 7 && board[colRowToPos(i, j)] == 0)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i--;
                            j++;
                        }
                    }
                }

            return moves;
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
        private int minMove(int[] board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);


            List<Move> sucessors;
            Move move;
            int[] nextBoard;
            int value;

            Debug.WriteLine("Min node at depth : " + depth + " with alpha : " + alpha +
                                " beta : " + beta);

            sucessors = LegalMoves(board);
          
            while (mayPlay(sucessors))
            {
                move = sucessors[0];
                sucessors.RemoveAt(0);
                nextBoard = (int[])board.Clone();
                Move(ref nextBoard, move);
                value = maxMove(nextBoard, depth + 1, alpha, beta);

                if (value < beta)
                {
                    beta = value;
                    //Debug.WriteLine("Min value : " + value + " at depth : " + depth);
                }

                if (beta < alpha)
                {
                    //Debug.WriteLine("Min value with prunning : " + alpha + " at depth : " + depth);
                    //Debug.WriteLine(sucessors.length() + " sucessors left");
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
        private int eval(int[] board)
        {
            int colorKing;
            int colorForce = 0;
            int enemyForce = 0;
            int piece;

            if (color == 1)
                colorKing = 2;//Withe king
            else
                colorKing = -2;

            try
            {
                for (int i = 0; i < 32; i++)
                {
                    piece = board[i];

                    if (piece != 0)
                        if (piece == color || piece == colorKing)
                            colorForce += calculateValue(piece, i);
                        else
                            enemyForce += calculateValue(piece, i);
                }
            }
            catch (Exception e)
            {
                //Debug.WriteLine(bad.StackTrace);
                //Application.Exit();
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
        private bool cutOffTest(int[] board, int depth)
        {
            return depth > maxDepth || board.Length == 0;
        }
    }
}

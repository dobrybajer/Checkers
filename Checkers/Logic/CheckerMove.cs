﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkers.Model;

namespace Checkers.Logic
{
    public class CheckerMove
    {
        private ObservableCollection<CheckersPiece> Pieces;
        private Player CurrentPlayer;

        public CheckerMove(ObservableCollection<CheckersPiece> pieces, Player player)
        {
            Pieces = pieces;
            CurrentPlayer = player;
        }

        public void SetCurrentPlayer(Player player)
        {
            CurrentPlayer = player;
        }

        public bool isValidMove(int from, int to)
        {
            if (from < 0 || from > 32 || to < 0 || to > 32)
                return false;

            if (Pieces[from].Type == PieceType.Free || Pieces[to].Type != PieceType.Free)
                return false;

            if (Pieces[from].Player != CurrentPlayer)
                return false;

            Player color = Pieces[from].Player;
            Player enemy;

            if (color == Player.White)
                enemy = Player.Black;
            else
                enemy = Player.White;

            int fromRow = posToRow(from);
            int fromCol = posToCol(from);
            int toRow = posToRow(to);
            int toCol = posToCol(to);

            int incX, incY;

            if (fromCol > toCol)
                incX = -1;
            else
                incX = 1;

            if (fromRow > toRow)
                incY = -1;
            else
                incY = 1;

            int x = fromCol + incX;
            int y = fromRow + incY;

            if (Pieces[from].Type == PieceType.Pawn)
            {
                bool goodDir;

                if ((incY == 1 && color == Player.White) || (incY == -1 && color == Player.Black))
                    goodDir = true;
                else
                    goodDir = false;

                if (x == toCol && y == toRow)
                    return goodDir;// && !mustAttack ();

                // If it wasn't a simple move it can only be an attack move
                return goodDir && x + incX == toCol && y + incY == toRow && Pieces[colRowToPos(x, y)].Player == enemy;
            }
            else // queen
            {
                while (x != toCol && y != toRow && Pieces[colRowToPos(x, y)].Type == PieceType.Free)
                {
                    x += incX;
                    y += incY;
                }

                // Simple move with a king piece
                if (x == toCol && y == toRow)
                    return false;// !mustAttack();

                if (Pieces[colRowToPos(x, y)].Player == enemy)
                {
                    x += incX;
                    y += incY;

                    while (x != toCol && y != toRow && Pieces[colRowToPos(x, y)].Type == PieceType.Free)
                    {
                        x += incX;
                        y += incY;
                    }

                    if (x == toCol && y == toRow)
                        return true;
                }
            }

            return false;
        }

        private List<Move> LegalMoves()
        {
            Player color, enemy;

            color = CurrentPlayer;
            if (color == Player.White)
                enemy = Player.Black;
            else
                enemy = Player.Black;

            return generateMoves(color, enemy);
        }

        private int posToCol(int value)
        {
            return (value % 4) * 2 + ((value / 4) % 2 != 0 ? 1 : 0);
        }

        private int posToRow(int value)
        {
            return value / 4;
        }

        private int colRowToPos(int col, int line)
        {
            if (isEven(line))
                return line * 4 + (col + 1) / 2;
            else
                return line * 4 + col / 2;
        }

        private bool isEven(int value)
        {
            return value % 2 == 0;
        }

        private List<Move> generateMoves(Player color, Player enemy)
        {
            List<Move> moves = new List<Move>();

            for (int k = 0; k < Pieces.Count; k++)
                if (Pieces[k].Player == CurrentPlayer)
                {
                    int x = posToCol(k);
                    int y = posToRow(k);
                    int i, j;

                    if (Pieces[k].Type == PieceType.Pawn)
                    {  // Simple piece
                        i = (color == Player.White) ? -1 : 1;

                        // See the diagonals /^ e \v
                        if (x < 7 && y + i >= 0 && y + i <= 7 && Pieces[colRowToPos(x + 1, y + i)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(x + 1, y + i)));
                        }

                        // See the diagonals ^\ e v/
                        if (x > 0 && y + i >= 0 && y + i <= 7 &&
                            Pieces[colRowToPos(x - 1, y + i)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(x - 1, y + i)));
                        };
                    }
                    else
                    { // It's a king piece
                        // See the diagonal \v
                        i = x + 1;
                        j = y + 1;

                        while (i <= 7 && j <= 7 && Pieces[colRowToPos(i, j)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i++;
                            j++;
                        }


                        // See the diagonals ^\
                        i = x - 1;
                        j = y - 1;
                        while (i >= 0 && j >= 0 && Pieces[colRowToPos(i, j)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i--;
                            j--;
                        }

                        // See the diagonals /^
                        i = x + 1;
                        j = y - 1;
                        while (i <= 7 && j >= 0 && Pieces[colRowToPos(i, j)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i++;
                            j--;
                        }

                        // See the diagonals v/
                        i = x - 1;
                        j = y + 1;
                        while (i >= 0 && j <= 7 && Pieces[colRowToPos(i, j)].Type == PieceType.Free)
                        {
                            moves.Add(new Move(k, colRowToPos(i, j)));

                            i--;
                            j++;
                        }
                    }
                }

            return moves;
        }
    }
}
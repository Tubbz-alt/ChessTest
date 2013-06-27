using System;

/*
namespace ChessTest.IA
{
    class SimpleIA
    {
        static public Move ComputeBestMove()
        {
            System.Collections.ArrayList moves = Board.GenerateValidMoves(PiecesColors.BLACK);
            //moves.Sort(new SortByScore());

            int maxVal = int.MinValue;
            Move bestMove = new Move { };
            foreach (Move move in moves)
            {
                char[,] backBoard = new char[8, 8];
                Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                int c = move.From / 8, r = move.From % 8, c2 = move.To / 8, r2 = move.To % 8;
                Board.pieces[c2, r2] = Board.pieces[c, r];
                Board.pieces[c, r] = '\0';
                int points = min(0);
                if (points > maxVal)
                {
                    maxVal = points;
                    bestMove = move;
                }
                Array.Copy(backBoard, Board.pieces, backBoard.Length);
            }

            return bestMove;
        }

        static public int min(int depth)
        {
            if (depth == 3)
            {
                return BoardEvaluator.GetBoardScore(PiecesColors.BLACK);
            }

            int minVal = int.MaxValue;
            System.Collections.ArrayList moves = Board.GenerateValidMoves(PiecesColors.WHITE, (depth == 2 ? true : false));
            if (depth == 2)
            {
                moves.Sort(new SortByScore());
                moves.Reverse();
                moves.RemoveRange(3, moves.Count - 4);
            }

            foreach (Move move in moves)
            {
                char[,] backBoard = new char[8, 8];
                Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                int c = move.From / 8, r = move.From % 8, c2 = move.To / 8, r2 = move.To % 8;
                Board.pieces[c2, r2] = Board.pieces[c, r];
                Board.pieces[c, r] = '\0';
                int points = max(depth + 1);
                if (points < minVal)
                {
                    minVal = points;
                }
                Array.Copy(backBoard, Board.pieces, backBoard.Length);
            }

            return minVal;
        }

        static int max(int depth)
        {
            if (depth == 3)
            {
                return BoardEvaluator.GetBoardScore(PiecesColors.BLACK);
            }

            int maxVal = int.MinValue;
            System.Collections.ArrayList moves = Board.GenerateValidMoves(PiecesColors.BLACK, (depth == 1 ? true : false));

            if (depth == 1)
            {
                moves.Sort(new SortByScore());
                moves.RemoveRange(3, moves.Count - 4);
            }

            foreach (Move move in moves)
            {
                char[,] backBoard = new char[8, 8];
                Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                int c = move.From / 8, r = move.From % 8, c2 = move.To / 8, r2 = move.To % 8;
                Board.pieces[c2, r2] = Board.pieces[c, r];
                Board.pieces[c, r] = '\0';
                int points = min(depth + 1);
                if (points > maxVal)
                {
                    maxVal = points;
                }
                Array.Copy(backBoard, Board.pieces, backBoard.Length);
            }

            return maxVal;
        }

        class SortByScore : System.Collections.IComparer
        {
            int System.Collections.IComparer.Compare(object a, object b)
            {
                Move ma = (Move)a;
                Move mb = (Move)b;
                if (ma.Score < mb.Score)
                    return 1;
                if (ma.Score > mb.Score)
                    return -1;
                else
                    return 0;
            }
        }
    }
}

*/

namespace ChessTest.IA
{
    class SimpleIA
    {
        static public int Depth = 3;

        static public Move ComputeBestMove()
        {
            int alpha = -99999;
            int beta = 99999;
            int depth = SimpleIA.Depth;

            System.Collections.ArrayList moves = Board.GenerateValidMoves(PieceColor.BLACK, true);
            moves.Sort(new SortByScore());
            Move bestMove = new Move { };
            
            foreach (Move move in moves) {
                char[,] backBoard = new char[8, 8];
                Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                int c = move.From / 8, r = move.From % 8, c2 = move.To / 8, r2 = move.To % 8;
                Board.pieces[c2, r2] = Board.pieces[c, r];
                Board.pieces[c, r] = '\0';

                // PVS enhancement
                int value = -AlphaBeta(depth - 1, -alpha - 1, -alpha, PieceColor.WHITE);
                if (value > alpha && value < beta)
                {
                    value = -AlphaBeta(depth - 1, -beta, -alpha, PieceColor.WHITE);
                }

                if (value > alpha) {
                    alpha = value;
                    bestMove = move;
                }
                Array.Copy(backBoard, Board.pieces, backBoard.Length);
            }
            
            return bestMove;
        }

        static int AlphaBeta(int depth, int alpha, int beta, PieceColor color)
        {
            if (depth == 0)
                return BoardEvaluator.GetBoardScore(color);

            int value = -99999;
            bool pv = false;
            PieceColor enemyColor = color == PieceColor.WHITE ? PieceColor.BLACK : PieceColor.WHITE;
            
            if (depth >= 3)
            {
                value = -AlphaBeta(depth - (depth > 6 ? 3 : 2) - 1, -beta, -beta + 1, enemyColor);
                if (value >= beta)
                    return beta;
            }

            System.Collections.ArrayList moves = Board.GenerateValidMoves(color, true);
            moves.Sort(new SortByScore());
            foreach (Move move in moves) {
                char[,] backBoard = new char[8, 8];
                Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                int c = move.From / 8, r = move.From % 8, c2 = move.To / 8, r2 = move.To % 8;
                Board.pieces[c2, r2] = Board.pieces[c, r];
                Board.pieces[c, r] = '\0';
                if (pv)
                {
                    value = -AlphaBeta(depth - 1, -alpha - 1, -alpha, enemyColor);
                    if (value > alpha && value < beta)
                    {
                        value = -AlphaBeta(depth - 1, -beta, -alpha, enemyColor);
                    }
                }
                else
                {
                    value = -AlphaBeta(depth - 1, -beta, -alpha, enemyColor);
                }
                Array.Copy(backBoard, Board.pieces, backBoard.Length);
                
                if( value >= beta )
                    return beta;
                
                if (value > alpha)
                {
                    alpha = value;
                    pv = true;
                }
            }
            
            return alpha;
        }

        class SortByScore : System.Collections.IComparer
        {
            int System.Collections.IComparer.Compare(object a, object b)
            {
                Move ma = (Move)a;
                Move mb = (Move)b;
                if (ma.Score < mb.Score)
                    return 1;
                if (ma.Score > mb.Score)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
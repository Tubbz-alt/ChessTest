//
// Chess Game
//
// Copyright 2012 Jose Luis P. Cardenas
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ChessTest
{
    public class Board : Canvas
    {
        static public char[,] pieces = new char[8, 8];
        static public Hashtable _pieces = new Hashtable();
        static public bool whites = true;
        static public char[] ids = { 'k', 'q', 'b', 'n', 'r', 'p' };

        static Board()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Board), new FrameworkPropertyMetadata(typeof(Board)));
        }

        // i need this for update the gui -.-
        static public Piece GetPiece(int col, int row)
        {
            return _pieces[col + "x" + row] as Piece;
        }

        static public void Register(int col, int row, Piece pc)
        {
            string hs = col + "x" + row;
            _pieces.Remove(hs);
            _pieces.Add(hs, pc);
            char p = ids[(int)pc.Type];
            pieces[col, row] = pc.Color == PieceColor.BLACK ? char.ToUpper(p) : p;
        }

        public void Move(int acol, int arow, int bcol, int brow, bool intern = false)
        {
            Piece pc = GetPiece(acol, arow);
            if (!intern)
            {
                if (OnMove != null)
                    OnMove(acol, arow, bcol, brow, pc.Color);
                Board.whites = !Board.whites;
            }

            pieces[acol, arow] = '\0';
            Register(bcol, brow, pc);

            if (!Game.GameOver && (brow == 0 || brow == 7) &&
                pc.Type == PieceType.PAWN && (!Game.IsConnected || Game.MainColor == pc.Color))
            {
                Promote promote = new Promote(pc);
                promote.Closed += delegate(object s, System.EventArgs e)
                {
                    Game game = GetTopParent() as Game;
                    this.Dispatcher.BeginInvoke(new PromoteDelegate(game.OnPromote), new object[] { ((Promote)s).Piece });
                };
                promote.ShowDialog();
            }

            if (OnMoveComplete != null)
                OnMoveComplete(acol, arow, bcol, brow, pc.Color);
        }

        static public bool ValidMove(PieceType type, int col, int row, int dcol, int drow, out int capture, bool first = false)
        {
            capture = -1;
            char piece = '\0', cpiece = '\0';
            PieceColor color;
            int acol = System.Math.Abs(col - dcol);
            int arow = System.Math.Abs(row - drow);

            if (dcol > 7 || drow > 7 || (piece = pieces[col, row]) == '\0')
            {
                return false;
            }
            color = GetColor(piece);

            if ((cpiece = pieces[dcol, drow]) != '\0')
            {
                if (GetColor(cpiece) != color)
                    capture = dcol * 8 + drow;
                else
                    return false;
            }
            if (type == PieceType.QUEEN)
            {
                if (col != dcol && row != drow)
                    type = PieceType.BISHOP;
                else
                    type = PieceType.ROOK;
            }

            if (type == PieceType.KNIGHT &&
                !((arow + acol) == 3 && System.Math.Max(arow, acol) == 2))
            {
                return false;
            }
            else if (type == PieceType.BISHOP)
            {
                if (dcol == col || drow == row || acol != arow)
                    return false;
                else
                    return Blocked(col, row, dcol, drow) == -1;
            }
            else if (type == PieceType.ROOK)
            {
                if (drow != row && dcol != col)
                    return false;
                else
                    return Blocked(col, row, dcol, drow) == -1;
            }
            else if (type == PieceType.PAWN)
            {
                int ina = color == Game.MainColor ? -1 : 1;
                int ipos = color == Game.MainColor ? 6 : 1;
                int dif = row + ina;

                if (!(drow == dif && (capture == -1 && col == dcol || capture != -1 && acol == 1)
                    || (row == ipos && capture == -1 && drow == (dif + ina) && pieces[dcol, dif] == '\0' && col == dcol)))
                    return false;
            }
            else if (type == PieceType.KING)
            {
                // check castling
               /* if (first && row == drow && System.Math.Abs(col - dcol) == 2 && capture == -1)
                {
                    char rook = '\0';
                    int posrook = dcol > col ? 7 : 0;
                    if (Blocked(col, row, (col > dcol ? 1 : 6), row) != -1 || (rook = pieces[posrook, row]) == '\0'
                        || char.ToLower(rook) != 'r' || Board.GetColor(rook) != color)
                        return false;
                    
                    // move rook
                    int ncol = (col > dcol ? col - 1 : col + 1);
                    Board.pieces[ncol, row] = Board.pieces[posrook, row];
                    Board.pieces[posrook, row] = '\0';
                }
                else*/ if (!((acol + arow) == 1 || (acol + arow) == 2 && System.Math.Max(acol, arow) != 2))
                {
                    return false;
                }
            }

            return true;
        }

        static public int Blocked(int acol, int arow, int bcol, int brow)
        {
            int bl = -1;
            int ica = acol != bcol ? (bcol > acol ? 1 : -1) : 0;
            int icb = arow != brow ? (brow > arow ? 1 : -1) : 0;
            int tcol = acol + ica;
            int trow = arow + icb;

            while (true)
            {
                if (brow > arow && trow >= brow || brow < arow && trow <= brow
                             || bcol > acol && tcol >= bcol || bcol < acol && tcol <= bcol)
                    break;
                if (Board.pieces[tcol, trow] != '\0')
                {
                    bl = (tcol * 8) + trow;
                    break;
                }

                tcol += ica;
                trow += icb;
            }

            return bl;
        }

        public void Capture(Piece cpiece)
        {
            cpiece.Capture = true;
            Children.Remove(cpiece);

            if (cpiece.Type == PieceType.KING)
            {
                Game.GameOver = true;
                if (OnGameOver != null)
                    OnGameOver(cpiece.Color);
            }
        }

        static public bool CheckCheck(PieceColor Color)
        {
            char ck = Color == PieceColor.WHITE ? 'k' : 'K';
            int pos = GetPos(ck);
            int kc = pos / 8, kr = pos % 8;

            for (int i = 0; i < 64; i++)
            {
                int c = i / 8, r = i % 8;
                char p = pieces[c, r];
                if (p != '\0' && GetColor(p) != GetColor(ck))
                {
                    int capture;
                    if (ValidMove(GetType(p), c, r, kc, kr, out capture))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public ArrayList GenerateValidMoves(PieceColor color, bool calc = false)
        {
            ArrayList moves = new ArrayList(48);
            for (int i = 0; i < 64; i++)
            {
                int c = i / 8, r = i % 8;
                char p = Board.pieces[c, r];
                if (p != '\0' && GetColor(p) == color)
                {
                    for (int n = 0; n < 64; n++)
                    {
                        int c2 = n / 8, r2 = n % 8;
                        int capture = -1;
                        if (ValidMove(GetType(p), c, r, c2, r2, out capture, false))
                        {
                            int score = 0;
                            if (calc)
                            {
                                char[,] backBoard = new char[8, 8];
                                System.Array.Copy(Board.pieces, backBoard, Board.pieces.Length);
                                Board.pieces[c2, r2] = Board.pieces[c, r];
                                Board.pieces[c, r] = '\0';
                                score = BoardEvaluator.GetBoardScore(color);
                                if (capture != -1)
                                {
                                    score += (BoardEvaluator.PieceValue(Board.pieces[c2, r2]) - BoardEvaluator.PieceValue(p) / 10);
                                }
                                System.Array.Copy(backBoard, Board.pieces, backBoard.Length);
                            }

                            Move m = new Move { From = i, To = n, Score = score, Capture = capture };
                            moves.Add(m);
                        }
                    }
                }
            }

            return moves;
        }

        static public int GetPos(char p, int distinct = -1)
        {
            for (int i = 0; i < 64; i++)
                if (p == pieces[(i / 8), (i % 8)] && (distinct == -1 || i != distinct))
                    return i;
            return -1;
        }

        static public PieceColor GetColor(char c)
        {
            return char.IsLower(c) ? PieceColor.WHITE : PieceColor.BLACK;
        }

        static public PieceType GetType(char c)
        {
            return (PieceType)System.Array.IndexOf(ids, char.ToLower(c));
        }

        public void Reset()
        {
            Children.Clear();
            _pieces.Clear();
            for (int i = 0; i < 64; i++)
            {
                pieces[(i / 8), (i % 8)] = '\0';
            }
        }

        private Window GetTopParent()
        {
            DependencyObject dpParent = this.Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            } while (dpParent.GetType().BaseType != typeof(Window));

            return dpParent as Window;
        }

        public delegate void MoveDelegate(int acol, int arow, int bcol, int brow, PieceColor color);
        public MoveDelegate OnMove;
        public MoveDelegate OnMoveComplete;
        public delegate void GameOverDelegate(PieceColor color);
        public GameOverDelegate OnGameOver;
    }
}

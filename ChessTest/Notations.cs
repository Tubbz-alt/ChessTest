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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessTest
{
    class Notations
    {
        public static string toAlgebraicNotation(string s)
        {
            string an = null;
            
            int c1, r1, c2, r2;
            char piece;
            if (Notations.parse(s, out c1, out r1, out c2, out r2)
                && (piece = Board.pieces[c1, r1]) != '\0')
            {
                an = piece.ToString();
                PieceType type = Board.GetType(piece);
                PieceColor color = Board.GetColor(piece);
                if (Board.GetColor(piece) == PieceColor.WHITE)
                    an = an.ToLower();

                char dest = Board.pieces[c2, r2];
                if (dest != '\0' && Board.GetColor(dest) != color && Board.GetType(dest) != PieceType.KING) {
                    an += "x";
                }/*
                if (type != PieceType.PAWN && type > PieceType.QUEEN)
                {
                    int i = Board.GetPos(piece, c1 * 8 + r1);
                    if (i != -1)
                    {
                        int pc = i / 8;
                        int pr = i % 8;
                        int p2 = Board.pieces[pc, pr];
                        int capture = -1;
                        if (Board.ValidMove(type, pc, pr, c2, r2, out capture))
                        {
                            if (c1 == pc)
                                an += Math.Abs(r1 - 8);
                            else
                                an += (char)(c1 + 97);
                        }
                    }
                }*/

                an += string.Format("{0}{1}", (char)(c2 + 97), Math.Abs(r2 - 8));
                if (dest != '\0' && Board.GetType(dest) == PieceType.KING)
                    an += "++";
            }

            return an;
        }

        public static string fromAlgebraicNotation(string s)
        {
            //bool check = false;
            if (s.Contains("+")) {
                s = s.Replace("+", "");
               // check = true;
            }

            Queue<char> qc = new Queue<char>(s.ToArray());
            char c = qc.Count < 3 ? 'p' : qc.Dequeue();
           // bool capture = false;
            char amb = '\0';
            string nn = "";
            PieceColor color = char.IsLower(c) ? PieceColor.WHITE : PieceColor.BLACK;
            if (qc.Peek() == 'x') {
               // capture = true;
                qc.Dequeue();
            }

            if (qc.Count == 3)
                amb = qc.Dequeue();

            Func<int, int> n1 = (a) => {
                return Math.Abs(a-8);
            };

            int dcol = (((int)qc.Dequeue()) - 97);
            int drow = n1(Math.Abs(((int)qc.Dequeue()-'0')));

            int i = 0;
            if (c == 'p' || c == 'P')
            {
                for (int n = 0; n < 8; n++)
                {
                    if (Board.pieces[dcol, n] == c)
                    {
                        i = (dcol * 8) + n;
                    }
                }
            }
            else
            {
                i = Board.GetPos(c);
            }

            int col = i / 8;
            int row = i % 8;
            int capture;
            
            if (!Board.ValidMove(Board.GetType(c), col, row, dcol, drow, out capture)
                || amb != '\0' && (!char.IsDigit(amb) && (((int)amb) - 97) != col || (Math.Abs(((int)amb - '0') - 8)) != row))
            {
                if ((i = Board.GetPos(c, i)) != -1)
                {
                    col = i / 8;
                    row = i % 8;
                }
            }

            if (i != -1) {
                nn = string.Format("{0}x{1}:{2}x{3}", col, row, dcol, drow);
            }

            return nn;
        }

        public static bool parse(string s, out int c1, out int r1, out int c2, out int r2)
        {
            string[] p = s.Split(':');
            c1 = r1 = c2 = r2 = 0;
            if (p.Length == 2)
            {
                string[] p1 = p[0].Split('x');
                string[] p2 = p[1].Split('x');
                if (p1.Length == 2 && p2.Length == 2)
                {
                    c1 = int.Parse(p1[0]);
                    r1 = int.Parse(p1[1]);
                    c2 = int.Parse(p2[0]);
                    r2 = int.Parse(p2[1]);

                    return true;
                }
            }

            return false;
        }
    }
}

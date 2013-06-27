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
namespace ChessTest
{
    class BoardEvaluator
    {
        static short[] pknight = 
        {0, 4, 8, 10, 10, 8, 4, 0,
        4, 8, 16, 20, 20, 16, 8, 4,
        8, 16, 24, 28, 28, 24, 16, 8,
        10, 20, 28, 32, 32, 28, 20, 10,
        10, 20, 28, 32, 32, 28, 20, 10,
        8, 16, 24, 28, 28, 24, 16, 8,
        4, 8, 16, 20, 20, 16, 8, 4,
        0, 4, 8, 10, 10, 8, 4, 0};
        
        static short[] pbishop =
        {14, 14, 14, 14, 14, 14, 14, 14,
        14, 22, 18, 18, 18, 18, 22, 14,
        14, 18, 22, 22, 22, 22, 18, 14,
        14, 18, 22, 22, 22, 22, 18, 14,
        14, 18, 22, 22, 22, 22, 18, 14,
        14, 18, 22, 22, 22, 22, 18, 14,
        14, 22, 18, 18, 18, 18, 22, 14,
        14, 14, 14, 14, 14, 14, 14, 14};

        static short[] ppawn =
        {0, 0, 0, 0, 0, 0, 0, 0,
        4, 4, 4, 0, 0, 4, 4, 4,
        6, 8, 2, 10, 10, 2, 8, 6,
        6, 8, 12, 16, 16, 12, 8, 6,
        8, 12, 16, 24, 24, 16, 12, 8,
        12, 16, 24, 32, 32, 24, 16, 12,
        12, 16, 24, 32, 32, 24, 16, 12,
        0, 0, 0, 0, 0, 0, 0, 0};

        static public int GetBoardScore(PieceColor color)
        {
            int total = 0;
            for (int i = 0; i < 64; i++)
            {
                int c = i / 8, r = i % 8;
                char p = Board.pieces[c, r];
                if (p != '\0')
                {
                    PieceType type = Board.GetType(p);
                    PieceColor pcolor = Board.GetColor(p);
                    int result = 0;
                    switch (type)
                    {
                        case PieceType.KING:
                            result += 9000;
                            break;
                        case PieceType.QUEEN:
                            result += 1100;
                            result += pknight[i];
                            break;
                        case PieceType.BISHOP:
                            result += 315;
                            result += pbishop[i];
                            break;
                        case PieceType.KNIGHT:
                            result += 330;
                            result += pknight[i];
                            break;
                        case PieceType.ROOK:
                            result += 500;
                            break;
                        case PieceType.PAWN:
                            result += 100;
                            result += ppawn[i];
                            break;
                    }

                    total += (color == pcolor) ? result : -result;
                }
            }

            return total;
        }

        static public int PieceValue(char c)
        {
            int value = 0;
            switch (char.ToLower(c))
            {
                case 'k':
                    value = 9000;
                    break;
                case 'q':
                    value = 1100;
                    break;
                case 'b':
                    value = 315;
                    break;
                case 'n':
                    value = 330;
                    break;
                case 'r':
                    value = 500;
                    break;
                case 'p':
                    value = 100;
                    break;
            }

            return value;
        }
    }
}

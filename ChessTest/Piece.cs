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
using System.Drawing;
using System.Windows.Input;

namespace ChessTest 
{
    public class Piece : System.Windows.Controls.Image
    {
        static Bitmap tileWhites = Properties.Resources.whitepieces;
        static Bitmap tileBlack = Properties.Resources.blackpieces;
        Bitmap pieceImage;
        bool dragging = false;
        System.Windows.Media.Imaging.BitmapImage pieceImaging;

        public Piece(PieceType type, PieceColor color, int col, int row, Board board)
        {
            this.Col = col;
            this.Row = row;
            this.X = col * Game.TILESIZE;
            this.Y =  row * Game.TILESIZE;
            this.Color = color;
            this.Board = board;
            FirstMove = true;
            SetType(type);
            
            this.MouseDown += delegate(object s, MouseButtonEventArgs ev) {
                if (!Game.GameOver && ((!Game.IsConnected && Game.MyTurn(Color)) ||
                    (Game.IsConnected && Game.MainColor == Color && Game.MyTurn())))
                {
                    dragging = true;
                    this.Cursor = Cursors.Hand;
                    System.Windows.Controls.Canvas.SetZIndex(this, 1000);
                }
            };

            this.MouseUp += new MouseButtonEventHandler(image_MouseUp);
            
            this.MouseMove += new MouseEventHandler(image_MouseMove);

            this.MouseLeave += new MouseEventHandler(image_MouseMove);
        }
        
        void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
            this.Cursor = Cursors.Arrow;
            Move((int)Math.Ceiling((double)(this.X - 35) / 75), (int)Math.Ceiling((double)(this.Y - 35) / 75));
        }

        void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (dragging)
                {
                    System.Windows.Point p = e.GetPosition(Board);
                    this.SetPosition((this.X = (int)p.X - 35), (this.Y = (int)p.Y - 35));
                }
            }
            else
            {
                image_MouseUp(sender, null);
            }
        }

        public void SetType(PieceType type)
        {
            this.Type = type;
            
            pieceImaging = null;
            pieceImaging = new System.Windows.Media.Imaging.BitmapImage();
            Bitmap tile = this.Color == PieceColor.WHITE ? tileWhites : tileBlack;

            pieceImage = tile.Clone(new Rectangle(((int)this.Type) * 75, 0, 75, 75),
            System.Drawing.Imaging.PixelFormat.DontCare);

            byte[] data;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            pieceImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            
            pieceImaging.BeginInit();
            pieceImaging.StreamSource = stream;
            pieceImaging.EndInit();
            
            this.Source = pieceImaging;
            //stream.Close();
        }

        public bool Move(int col, int row)
        {
            bool valid = false;
            int capture = -1;
            if (Board.ValidMove(Type, Col, Row, col, row, out capture, FirstMove))
            {
                if (capture != -1)
                    Board.Capture(Board.GetPiece(capture / 8, capture % 8));

                Board.Move(this.Col, this.Row, col, row);
                this.Col = col;
                this.Row = row;

                valid = true;
            }

            this.SetPosition((this.X = this.Col * 75), (this.Y = this.Row * 75));

            System.Windows.Controls.Canvas.SetZIndex(this, 100);

            return valid;
        }
        
        public void SetPosition()
        {
            this.SetPosition(this.X, this.Y);
        }
        
        public void SetPosition(int x, int y)
        {
            System.Windows.Controls.Canvas.SetLeft(this, x);
            System.Windows.Controls.Canvas.SetTop(this, y);
        }

        public int Row
        {
            set;
            get;
        }

        public int Col
        {
            set;
            get;
        }

        public int X
        {
            set;
            get;
        }

        public int Y
        {
            set;
            get;
        }

        public PieceType Type
        {
            set;
            get;
        }

        public PieceColor Color
        {
            set;
            get;
        }

        public bool FirstMove
        {
            set;
            get;
        }

        public Board Board
        {
            set;
            get;
        }

        public bool Capture
        {
            set;
            get;
        }
    }
}
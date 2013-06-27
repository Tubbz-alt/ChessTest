using System;
using System.Resources;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using ChessTest.Sockets;

namespace ChessTest
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Window
    {
        public const int TILESIZE = 75;
        ArrayList tiles = new ArrayList();
        Computer computer = null;
        string result = "*";
        int historyMove = 1;
        GameType _lastState;
        System.Windows.Forms.Timer replayTimer = null;

        public Game()
        {
            tiles.Add(new BitmapImage(new Uri("/Images/lightTile.png", UriKind.Relative)));
            tiles.Add(new BitmapImage(new Uri("/Images/darkTile.png", UriKind.Relative)));

            MainColor = PieceColor.WHITE;
            Type = GameType.UNSTARTED;
            History = new ArrayList();

            InitializeComponent();

            DrawBoard();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InternalOnMove();

            canvas1.OnGameOver = (PieceColor gcolor) =>
            {
                gameoverLabel.Content = string.Format("{0} ha ganado!", (gcolor == PieceColor.BLACK ? "blancas" : "negras"));
                gameoverGrid.Visibility = Visibility.Visible;
                statusTextBlock.Text = "Juego terminado";
                result = gcolor == PieceColor.WHITE ? "1-0" : "0-1";
            };
        }

        public void InitBoard()
        {
            DrawBoard();
        }

        private void DrawBoard()
        {
            bool clight = true;
            bool sWhite = MainColor == PieceColor.BLACK;
            int left = 0;
            int top = 0;

            // reset
            canvas1.Reset();
            Board.whites = true;
            GameOver = false;

            uimoves.Text = "";
            gameoverGrid.Visibility = Visibility.Collapsed;

            for (int row = 0; row < 8; row++)
            {
                left = 0;
                if (row == 6)
                    sWhite = !sWhite;

                for (int col = 0; col < 8; col++)
                {
                    Image m = new Image();
                    m.Source = tiles[(clight ? 0 : 1)] as BitmapImage;

                    canvas1.Children.Add(m);
                    Canvas.SetLeft(m, left);
                    Canvas.SetTop(m, top);
                    left += TILESIZE;
                    clight = !clight;
                    PieceType type = PieceType.KING;
                    bool pb = false;

                    //
                    if (row == 0 || row == 7)
                    {
                        switch (col)
                        {
                            case 0:
                            case 7:
                                type = PieceType.ROOK;
                                break;
                            case 1:
                            case 6:
                                type = PieceType.KNIGHT;
                                break;
                            case 2:
                            case 5:
                                type = PieceType.BISHOP;
                                break;
                            default:
                                if (col == (MainColor == PieceColor.WHITE ? 3 : 4))
                                    type = PieceType.QUEEN;
                                break;
                        }
                        pb = true;
                    }
                    else if (row == 1 || row == 6)
                    {
                        type = PieceType.PAWN;
                        pb = true;
                    }

                    if (pb && Type != GameType.UNSTARTED)
                    {
                        Piece pc = new Piece(type, (sWhite ? PieceColor.WHITE : PieceColor.BLACK), col, row, canvas1);
                        canvas1.Children.Add(pc);
                        pc.SetPosition();
                        Canvas.SetZIndex(pc, 100);
                        Board.Register(pc.Col, pc.Row, pc);
                    }
                }

                clight = !clight;
                top += TILESIZE;
            }
        }

        public void StartNewGame()
        {
            NetworkDisconnect();
            MainColor = PieceColor.WHITE;
            ResetElements();

            if (Type == GameType.HUMAN_COMPUTER)
            {
                AttachComputer();
            }

            InitBoard();
        }

        public void StartGameOnline()
        {
            Type = GameType.HUMAN_ONLINE;
            MainColor = IsServer ? PieceColor.WHITE : PieceColor.BLACK;
            ResetElements();

            canvas1.OnMoveComplete = (int acol, int arow, int bcol, int brow, PieceColor color) =>
            {
                if (IsConnected && color == MainColor)
                {
                    string com = "MOV " + acol + "x" + arow + ":" + bcol + "x" + brow;
                    if (IsServer)
                        Server.SendMessage(com);
                    else
                        Client.SendMessage(com);
                }
                CheckAlert();
            };

            InitBoard();
        }

        void CheckAlert()
        {
            if (!Game.GameOver)
            {
                if (Board.CheckCheck(Board.whites ? PieceColor.WHITE : PieceColor.BLACK))
                {
                    gameoverLabel.Content = "Jaque!!";
                    gameoverGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    gameoverGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        void AttachComputer()
        {
            computer = new Computer();
            computer.OnMoveReady += delegate(Move bmove)
            {
                Piece pm = (Piece)Board._pieces[(bmove.From / 8) + "x" + (bmove.From % 8)];
                int c = bmove.To / 8, r = bmove.To % 8;
                canvas1.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    pm.Move(c, r);
                    statusTextBlock.Text = "Listo";

                    CheckAlert();
                }));
            };

            canvas1.OnMoveComplete += (int acol, int arow, int bcol, int brow, PieceColor color) =>
            {
                if (color == MainColor && Type != GameType.REPLAY)
                {
                    statusTextBlock.Text = "Pensando...";
                    computer.NextMove();
                }
            };
        }

        public void InternalOnMove()
        {
            canvas1.OnMove += (int acol, int arow, int bcol, int brow, PieceColor color) =>
            {
                Func<int, int, string> n1 = (a, b) =>
                {
                    return MainColor == PieceColor.WHITE ?
                      (char)(a + 97) + "x" + Fix(b - 1) : (char)(Fix(a) + 97) + "x" + (b + 1);
                };
                string nn = acol + "x" + arow + ":" + bcol + "x" + brow;
                string an = Notations.toAlgebraicNotation(nn);

                if (Type != GameType.REPLAY)
                    History.Add(new HMove { Nn = nn, An = an });

                uimoves.Text += (color == PieceColor.WHITE ? "\r\n" : " - ") + an;// (color == PieceColor.WHITE ? "\r\n" : " - ") + n1(acol, arow) + ":" + n1(bcol, brow);

                uimoves.ScrollToEnd();
            };
        }

        public void ResetElements()
        {
            computer = null;
            canvas1.OnMoveComplete = null;
            History.Clear();
            result = "*";
            Board.whites = true;
            playerGrid.Visibility = System.Windows.Visibility.Collapsed;
            MenuItem item = (MenuItem)((MenuItem)menu1.Items[1]).Items[1];
            item.Header = "Reproducir";
            item.IsEnabled = false;
        }

        public void NetworkDisconnect()
        {
            if (Server != null && Server.IsConnected)
                Server.Disconnect();
            if (Client != null && Client.IsConnected)
                Client.Disconnect();
        }

        public GameType Type
        {
            set;
            get;
        }

        static public PieceColor MainColor
        {
            set;
            get;
        }

        public Client Client
        {
            set;
            get;
        }

        public Server Server
        {
            set;
            get;
        }

        static public bool IsServer
        {
            set;
            get;
        }

        static public bool IsConnected
        {
            set;
            get;
        }

        static public bool GameOver
        {
            set;
            get;
        }

        public ArrayList History
        {
            set;
            get;
        }

        public int HistoryMove
        {
            set
            {
                historyMove = (value < 1 || value > History.Count) ? 1 : value;
            }
            get
            {
                return historyMove;
            }
        }

        static public bool MyTurn(PieceColor color)
        {
            int whites = Board.whites ? 0 : 1;
            return ((whites - (int)color) == 0);
        }

        static public bool MyTurn()
        {
            return Game.MyTurn(Game.MainColor);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            NewGame newWindow = new NewGame(this);
            newWindow.ShowDialog();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            File file = new File();
            file.Save(History, result, Type);
        }

        private void Replay_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)((MenuItem)menu1.Items[1]).Items[1];
            if (Type != GameType.REPLAY)
            {
                _lastState = Type;
                Type = GameType.REPLAY;
                playerGrid.Visibility = System.Windows.Visibility.Visible;
                item.Header = "Detener";
                item.IsEnabled = true;
                prevBtn.IsEnabled = false;
                HistoryMove = 1;
                MoveFromHistory(History, HistoryMove);
                replayTimer = null;
                replayTimer = new System.Windows.Forms.Timer();
                replayTimer.Interval += 3000;
                replayTimer.Tick += new EventHandler(replayTimerTask);
                replayTimer.Enabled = true;
            }
            else
            {
                playerGrid.Visibility = System.Windows.Visibility.Collapsed;
                item.Header = "Reproducir";
                item.IsEnabled = false;
                MoveFromHistory(History, History.Count);
                Type = _lastState;
                replayTimer = null;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            File file = new File();
            File.GameLoaded gloaded = file.Load();
            if (gloaded.Tags != null)
            {
                Type = GameType.HUMAN_HUMAN;
                ResetElements();
                canvas1.OnMove = null;
                History = gloaded.History;
                MoveFromHistory(History, History.Count);

                if (((string)gloaded.Tags["black"]).ToLower().Equals("computer"))
                {
                    Type = GameType.HUMAN_COMPUTER;
                    AttachComputer();
                }

                ((MenuItem)((MenuItem)menu1.Items[1]).Items[1]).IsEnabled = true;
                InternalOnMove();
            }
        }

        public void MoveFromHistory(ArrayList history, int n)
        {
            InitBoard();
            for (int i = 0; i < n; i++)
            {
                HMove move = (HMove)history[i];
                if (move.Nn == string.Empty)
                    move.Nn = Notations.fromAlgebraicNotation(move.An);
                int c, r, c2, r2;
                Notations.parse(move.Nn, out c, out r, out c2, out r2);
                Piece p = (Piece)Board._pieces[c + "x" + r];
                p.Move(c2, r2);
                Board.whites = !Board.whites;
            }
        }
        
        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryMove <= 2)
                prevBtn.IsEnabled = false;

            HistoryMove--;
            MoveFromHistory(History, HistoryMove);
            nextBtn.IsEnabled = true;
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryMove >= History.Count-1) {
                nextBtn.IsEnabled = false;
                if (replayTimer != null && replayTimer.Enabled)
                    pauseBtn_Click(null, null);
            }

            HistoryMove++;
            MoveFromHistory(History, HistoryMove);
            prevBtn.IsEnabled = true;
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (replayTimer != null)
            {
                replayTimer.Enabled = !replayTimer.Enabled;
            }
        }

        private void replayTimerTask(object sender, EventArgs e)
        {
            nextBtn_Click(null, null);
        }

        public void OnPromote(Piece piece)
        {
            if (IsConnected)
            {
                string com = "PRM " + piece.Col + "x" + piece.Row + ":" + (int)piece.Type;
                if (IsServer)
                    Server.SendMessage(com);
                else
                    Client.SendMessage(com);
            }
        }

        public void ChangeStatusBox(string text)
        {
            statusTextBlock.Text = text;
            statusTextBlock.InvalidateArrange();
        }

        public void DataReceivedHandler(string msg)
        {
            string com = msg.Substring(0, 3);
            Match match;

            if (com == "MOV")
            {
                match = Regex.Match(msg, @"^MOV ([0-9]+)x([0-9]+):([0-9]+)x([0-9]+)$");
                if (match.Success)
                {
                    int acol = Fix(int.Parse(match.Groups[1].Value));
                    int arow = Fix(int.Parse(match.Groups[2].Value));
                    int bcol = Fix(int.Parse(match.Groups[3].Value));
                    int brow = Fix(int.Parse(match.Groups[4].Value));
                    Piece pc;
                    if ((pc = Board.GetPiece(acol, arow)) != null)
                    {
                        if (pc.Color != MainColor)
                        {
                            pc.Move(bcol, brow);
                        }
                    }
                }
            }
            else if (com == "PRM")
            {
                match = Regex.Match(msg, @"^PRM ([0-9]+)x([0-9]+):([0-9]+)$");
                if (match.Success)
                {
                    int acol = Fix(int.Parse(match.Groups[1].Value));
                    int arow = Fix(int.Parse(match.Groups[2].Value));
                    int type = int.Parse(match.Groups[3].Value);
                    Piece pc;
                    if ((pc = Board.GetPiece(acol, arow)) != null)
                    {
                        pc.SetType((PieceType)type);
                    }
                }
            }
        }

        static public int Fix(int a)
        {
            return Math.Abs(a - 7);
        }

        private void Acercade_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Visible = true;
        }
    }

    public delegate void StartGameOnlineDelegate();
    public delegate void DataReceivedDelegate(string msg);
    public delegate void ChangeStatusBoxDelegate(string text);
    public delegate void PromoteDelegate(Piece piece);
}
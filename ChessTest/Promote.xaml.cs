using System.Windows;

namespace ChessTest
{
    /// <summary>
    /// Interaction logic for Promote.xaml
    /// </summary>
    public partial class Promote : Window
    {
        public Promote(Piece piece)
            : this()
        {
            this.Piece = piece;
        }

        public Promote()
        {
            InitializeComponent();
        }

        private void promotequeen_Click(object sender, RoutedEventArgs e)
        {
            SetType(PieceType.QUEEN);
        }

        private void promoterook_Click(object sender, RoutedEventArgs e)
        {
            SetType(PieceType.ROOK);
        }

        private void promotebishop_Click(object sender, RoutedEventArgs e)
        {
            SetType(PieceType.BISHOP);
        }

        private void promoteknight_Click(object sender, RoutedEventArgs e)
        {
            SetType(PieceType.KNIGHT);
        }

        private void SetType(PieceType type)
        {
            Piece.SetType(type);

            Close();
        }

        public Piece Piece
        {
            set;
            get;
        }
    }
}

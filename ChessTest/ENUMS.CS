namespace ChessTest
{
    public enum PieceType
    {
        KING,
        QUEEN,
        BISHOP,
        KNIGHT,
        ROOK,
        PAWN
    }

    public enum PieceColor
    {
        WHITE,
        BLACK
    }

    public enum GameType
    {
        HUMAN_HUMAN,
        HUMAN_COMPUTER,
        HUMAN_ONLINE,
        REPLAY,
        UNSTARTED
    }

    public struct Move
    {
        public int From;
        public int To;
        public int Score;
        public int Capture;
    }

    public struct HMove
    {
        public string Nn;
        public string An;
    }
}

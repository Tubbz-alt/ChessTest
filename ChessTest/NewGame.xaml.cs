using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessTest
{
    /// <summary>
    /// Interaction logic for NewGame.xaml
    /// </summary>
    public partial class NewGame : Window
    {
        Game game;

        public NewGame(Game game)
        {
            this.game = game;
            InitializeComponent();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            game.Type = (GameType)comboBox1.SelectedIndex;

            if (game.Type == GameType.HUMAN_ONLINE)
            {
                Connect connectWin = new Connect(game);
                connectWin.ShowDialog();
            }
            else
            {
                IA.SimpleIA.Depth = comboBox2.SelectedIndex == 0 ? 3 : 5;
                game.StartNewGame();
            }

            Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canvas1 != null)
                canvas1.Visibility = comboBox1.SelectedIndex == 1 ? Visibility.Visible : Visibility.Hidden;
        }
    }
}

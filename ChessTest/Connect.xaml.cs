using System.Windows;
using ChessTest.Sockets;

namespace ChessTest
{
    /// <summary>
    /// Interaction logic for Connect.xaml
    /// </summary>
    public partial class Connect : Window
    {
        Game game;

        public Connect(Game game) : this()
        {
            this.game = game;
        }

        public Connect()
        {
            InitializeComponent();
        }

        private void btnlisten_Click(object sender, RoutedEventArgs e)
        {
            game.NetworkDisconnect();
            game.Server = new Server();
            game.Server.StartListening();
            Game.IsServer = true;

            SetUIStatus("Esperando conexiones...");
            game.Server.ConnectionBegin += delegate(object s, System.EventArgs ev) {
                Game.IsConnected = true;
                SetUIStatus("Conectado cliente, empiecen!");
                
                this.Dispatcher.BeginInvoke(new StartGameOnlineDelegate(game.StartGameOnline), new object[] { });
                game.Server.OnDataReceived += delegate(string msg) {
                    this.Dispatcher.BeginInvoke(new DataReceivedDelegate(game.DataReceivedHandler), new object[] { msg });
                };
            };

            game.Server.ConnectionClose += delegate(object s, System.EventArgs ev) { Game.IsConnected = false; SetUIStatus("Desconectado"); };

            MessageBox.Show("Recibiendo conexiones en el puerto " + game.Server.Port, "", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void bntconnect_Click(object sender, RoutedEventArgs e)
        {
            System.Net.IPAddress addr;
            if (System.Net.IPAddress.TryParse(iptarget.Text, out addr))
            {
                game.NetworkDisconnect();
                game.Client = new Client();
                MessageBox.Show("Conectando...", "", MessageBoxButton.OK, MessageBoxImage.Information);
                SetUIStatus("Esperando al servidor...");

                game.Client.ConnectionBegin += (s, ev) => {
                    Game.IsConnected = true;
                    Game.IsServer = false;
                    SetUIStatus("Conectado, empieza tu amigo");

                    this.Dispatcher.BeginInvoke(new StartGameOnlineDelegate(game.StartGameOnline), new object[] { });
                    game.Client.OnDataReceived += (msg) => 
                        this.Dispatcher.BeginInvoke(new DataReceivedDelegate(game.DataReceivedHandler), new object[] {msg});
                };

                game.Client.ConnectionClose += (s, ev) => { Game.IsConnected = false; SetUIStatus("Desconectado"); };

                game.Client.Connect(iptarget.Text);

                Close();
            }
            else
            {
                MessageBox.Show("Dirección IP invalida");
            }
        }

        void SetUIStatus(string status)
        {
            this.Dispatcher.BeginInvoke(new ChangeStatusBoxDelegate(game.ChangeStatusBox), new object[] { status });
        }
    }
}
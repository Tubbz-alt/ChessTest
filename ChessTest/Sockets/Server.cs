using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChessTest.Sockets
{
    public class Server : SocketAbstract
    {
        const int MAX_CLIENTS = 1;

        public Socket[] workerSocket = new Socket[10];
        private int clientCount = 0;

        public void StartListening()
        {
            StartListening(PORT_DEFAULT);
        }

        public void StartListening(int port)
        {
            Port = port;

            if (socket != null && socket.Connected)
            {
                return;
            }

            try
            {
                socket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Port);

                socket.Bind(ipLocal);

                socket.Listen(4);

                socket.BeginAccept(new AsyncCallback(EndConnectCallback), null);
            }
            catch (SocketException)
            {
                if (socket != null && socket.Connected) socket.Close();

                OnDisconnected();
            }
            catch (ObjectDisposedException)
            {
                OnDisconnected();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            finally
            {

            }
        }
        
        public override void  Disconnect()
        {
            clientCount = 0;
 	        base.Disconnect();
        }

        public void SendMessage(string data)
        {
            SendSocketData(System.Text.ASCIIEncoding.ASCII.GetBytes(data));
        }

        protected void SendSocketData(byte[] data)
        {
            SendSocketData(workerSocket[0], data);
        }

        public int Port
        {
            set;
            get;
        }

        protected override void EndConnectCallback(IAsyncResult asyn)
        {
            try
            {
                if (clientCount >= MAX_CLIENTS)
                    throw new Exception("Cliente rechazado, maximo superado");

                workerSocket[clientCount] = socket.EndAccept(asyn);

                OnConnected();

                BeginDataReceive(workerSocket[clientCount]);

                ++clientCount;

                socket.BeginAccept(new AsyncCallback(EndConnectCallback), null);
            }
            catch (Exception se)
            {
                System.Diagnostics.Debug.WriteLine(se.Message);
            }
        }

        protected override void EndReceiveCallback(IAsyncResult asyn)
        {
            int cnt = 0;
            try
            {
                Socket socket = (Socket)asyn.AsyncState;

                cnt = socket.EndReceive(asyn);
                if (cnt == 0)
                {
                    OnDisconnected();
                    return;
                }

                string msg;
                using (BinaryReader reader = new BinaryReader(new MemoryStream(socketBuffer, 0, cnt)))
                {
                    msg = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(cnt));
                }

                OnMessageReceived(msg);

                BeginDataReceive(socket);
            }
            catch (Exception e)
            {
                if (socket != null && socket.Connected) socket.Close();

                System.Diagnostics.Debug.WriteLine(e.Message);

                OnDisconnected();
            }
        }

        protected override void OnMessageReceived(string msg)
        {
            if (OnDataReceived != null)
                OnDataReceived(msg);
        }
    }
}
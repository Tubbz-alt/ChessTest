using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChessTest.Sockets
{
    public abstract class SocketAbstract
    {
        protected Socket socket;
        protected byte[] socketBuffer = new byte[1500];
        protected const int PORT_DEFAULT = 19986;

        public virtual void Disconnect()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;

                OnDisconnected();
            }
        }

        protected virtual void EndConnectCallback(IAsyncResult ar)
        {
            try
            {
                ((Socket)socket).EndConnect(ar);

                OnConnected();

                BeginDataReceive(socket);
            }
            catch (Exception e)
            {
                OnDisconnected();
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        protected virtual void OnConnected()
        {
            if (ConnectionBegin != null)
                ConnectionBegin(this, new EventArgs());
        }

        protected virtual void OnDisconnected()
        {
            if (ConnectionClose != null)
                ConnectionClose(this, new EventArgs());
        }

        protected virtual void OnMessageReceived(string msg)
        {
        }

        protected virtual void SendSocketData(Socket socket, byte[] data)
        {
            try
            {
                lock (socket)
                {
                    socket.Send(data, 0, data.Length, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        protected virtual void BeginDataReceive(Socket socket)
        {
            socketBuffer = new byte[socketBuffer.Length];
            socket.BeginReceive(socketBuffer, 0, socketBuffer.Length, SocketFlags.None, new AsyncCallback(EndReceiveCallback), socket);
        }

        protected virtual void EndReceiveCallback(IAsyncResult ar)
        {
            int cnt = 0;
            try
            {
                Socket socket = (Socket)ar.AsyncState;

                cnt = socket.EndReceive(ar);

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

        public bool IsConnected
        {
            get { return socket.Connected; }
        }

        public event EventHandler ConnectionBegin;

        public event EventHandler ConnectionClose;
         
        public delegate void DataReceivedDelegate(string msg);

        public DataReceivedDelegate OnDataReceived;
    }
}
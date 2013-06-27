using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChessTest.Sockets
{
    public class Client : SocketAbstract
    {
        public void Connect(string ip)
        {
            this.Connect(ip, PORT_DEFAULT);
        }

        public void Connect(string ip, int port)
        {
            if (socket != null && socket.Connected)
            {
                return;
            }

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress hostIP = null;
                if (IPAddress.TryParse(ip, out hostIP))
                {
                    ((Socket)socket).BeginConnect(new System.Net.IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(EndConnectCallback), socket);
                }
                else
                {
                    ((Socket)socket).BeginConnect(ip, port, new AsyncCallback(EndConnectCallback), socket);
                }
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

        public void SendMessage(string data)
        {
            SendSocketData(System.Text.ASCIIEncoding.ASCII.GetBytes(data));
        }

        protected void SendSocketData(byte[] data)
        {
            SendSocketData(socket, data);
        }

        protected override void OnMessageReceived(string msg)
        {
            if (OnDataReceived != null)
                OnDataReceived(msg);
        }
    }
}
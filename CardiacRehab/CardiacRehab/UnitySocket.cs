using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardiacRehab
{
    class UnitySocket
    {
        public int portNumber;
        public Socket unitySocketListener;
        public Socket unitySocketWorker = null;

        public UnitySocket(int Port)
        {
            portNumber = Port;
        }

        public void ConnectToUnity()
        {
            try
            {
                unitySocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //unitySocketListener.NoDelay = true;
                IPAddress addy = System.Net.IPAddress.Parse("127.0.0.1");
                IPEndPoint iplocal = new IPEndPoint(addy, portNumber);
                //bind to local IP Address
                unitySocketListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                //unitySocketListener.Listen(4);

                //unitySocketListener.BeginConnect(iplocal, new AsyncCallback(OnUnitySocketConnection), null);

                //create call back for client connections -- aka maybe recieve video here????
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at ConnectToUnity");
                Console.WriteLine(e.Message);
            }
        }

        private void OnUnitySocketConnection(IAsyncResult asyn)
        {
            try
            {
                unitySocketWorker = unitySocketListener.EndAccept(asyn);
                //unitySocketWorker.NoDelay = true;
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("OnSocketConnection: Socket has been closed", "error");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message, "error");
            }
        }

        public void CloseSocket()
        {
            if (unitySocketWorker != null)
            {
                if (unitySocketWorker.Connected)
                {
                    unitySocketWorker.Close();
                }

                if (unitySocketListener.Connected)
                {
                    unitySocketListener.Close();
                }
            }
        }
    }
}

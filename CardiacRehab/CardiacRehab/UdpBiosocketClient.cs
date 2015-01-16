using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardiacRehab
{
    class UdpBiosocketClient
    {
        //private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker = null;
        String IpAddress;
        int PortNumber;
        PatientWindow window;
        int patientindex;
        int patientDbId;
        int sessionId;
        byte[] udpDataBuffer = new byte[256];

        public UdpBiosocketClient(String ip, int port, int index, int dbId, int session, PatientWindow currentwindow)
        {
            IpAddress = ip;
            PortNumber = port;
            patientindex = index;
            window = currentwindow;
            patientDbId = dbId;
            sessionId = session;
        }

        public void InitializeBioSockets()
        {
            try
            {
                //create listening socket
                //Console.WriteLine("InitializeBioSockets1");
                socketBioListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //Console.WriteLine("InitializeBioSockets2");
                IPAddress addy = IPAddress.Parse(IpAddress);
                //Console.WriteLine("InitializeBioSockets3");
                EndPoint iplocal = new IPEndPoint(addy, PortNumber);
                //Console.WriteLine("InitializeBioSockets4");
                //bind to local IP Address
                socketBioListener.Bind(iplocal);
                //Console.WriteLine("InitializeBioSockets5");

                socketBioListener.BeginSendTo(udpDataBuffer, 0, udpDataBuffer.Length, SocketFlags.None, iplocal, new AsyncCallback(DoSend), socketBioListener);

                //when connected, send...

                //socketBioListener.BeginReceiveFrom(udpDataBuffer, 0, udpDataBuffer.Length, SocketFlags.None, ref iplocal, DoReceiveFrom, socketBioListener);
            }
            catch (SocketException e)
            {
                //something went wrong
                Console.WriteLine("SocketException thrown at InitializeBiosockets");
                MessageBox.Show(e.Message);
            }

        }

        private void DoSend(IAsyncResult iar)
        {
            try
            {
                Console.WriteLine("send data");

                //Do other, more interesting, things with the received message.
            }
            catch (ObjectDisposedException)
            {
                //expected termination exception on a closed socket.
                // ...I'm open to suggestions on a better way of doing this.
            }
        }

        public void CloseSocket()
        {
            if (bioSocketWorker != null)
            {
                if (bioSocketWorker.Connected)
                {
                    bioSocketWorker.Close();
                }

                if (socketBioListener.Connected)
                {
                    socketBioListener.Close();
                }
            }
        }
    }
}

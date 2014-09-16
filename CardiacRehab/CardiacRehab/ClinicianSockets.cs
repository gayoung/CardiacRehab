using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardiacRehab
{
    class CliniSocketPacket
    {
        public Socket packetSocket;
        public byte[] dataBuffer = new byte[200];
    }

    class ClinicianSockets
    {
        private AsyncCallback cliniSocketWorkerCallback;
        public Socket cliniSocketListener;
        public Socket cliniSocketWorker = null;

        // doctor IP
        String IpAddress;
        int PortNumber;
        DoctorWindow window;

        // All bio data were inserted into the database when the patient received it from the
        // phone. So just need to display the data for the clinician side.
        public ClinicianSockets(String ip, int port, DoctorWindow currentwindow)
        {
            IpAddress = ip;
            PortNumber = port;
            window = currentwindow;
        }

        public void InitializeCliniSockets()
        {
            try
            {
                //create listening socket
                cliniSocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                cliniSocketListener.NoDelay = true;
                IPAddress addy = IPAddress.Parse(IpAddress);
                IPEndPoint iplocal = new IPEndPoint(addy, PortNumber);
                //bind to local IP Address
                cliniSocketListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                cliniSocketListener.Listen(4);
                //create call back for client connections
                cliniSocketListener.BeginAccept(new AsyncCallback(OnCliniSocketConnection), null);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at InitializeCliniSockets");
                MessageBox.Show(e.Message);
            }

        }
        private void OnCliniSocketConnection(IAsyncResult asyn)
        {
            try
            {
                cliniSocketWorker = cliniSocketListener.EndAccept(asyn);
                cliniSocketWorker.NoDelay = true;
                WaitForCliniData(cliniSocketWorker);
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\n OnCliniSocketConnection: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at OnCliniSocketConnection");
                MessageBox.Show(e.Message);
            }

        }
        private void WaitForCliniData(Socket soc)
        {
            try
            {
                if (cliniSocketWorkerCallback == null)
                {
                    cliniSocketWorkerCallback = new AsyncCallback(OnCliniDataReceived);
                }

                CliniSocketPacket sockpkt = new CliniSocketPacket();
                soc.NoDelay = true;
                sockpkt.packetSocket = soc;
                //start listening for data
                soc.BeginReceive(sockpkt.dataBuffer, 0, sockpkt.dataBuffer.Length, SocketFlags.None, cliniSocketWorkerCallback, sockpkt);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at WaitForCliniData");
                MessageBox.Show(e.Message);
            }
        }

        private void OnCliniDataReceived(IAsyncResult asyn)
        {
            try
            {
                CliniSocketPacket socketID = (CliniSocketPacket)asyn.AsyncState;
                //end receive
                int end = 0;
                end = socketID.packetSocket.EndReceive(asyn);

                //Console.WriteLine(end.ToString());

                // The Connection between the clinician and the patient has ended. Try to reconnect
                // (probably need to be changed after testing...)
                if (end == 0)
                {
                    socketID.packetSocket.Close();
                    cliniSocketListener.Close();
                    InitializeCliniSockets();
                }
                // patient is connected
                else
                {
                    char[] chars = new char[end + 1];
                    Decoder d = Encoding.UTF8.GetDecoder();
                    int len = d.GetChars(socketID.dataBuffer, 0, end, chars, 0);
                    String receivedData = new String(chars);

                    window.UpdateUI(receivedData, PortNumber);

                    WaitForCliniData(cliniSocketWorker);
                }
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\nOnCliniDataReceived: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException at OnCliniDataReceived");
                // this error is thrown when the doctor disconnects
                // need to add code to close sockets and close the application
                MessageBox.Show(e.Message);
            }

        }

        public void CloseSocket()
        {
            if(cliniSocketWorker != null)
            {
                if (cliniSocketWorker.Connected)
                {
                    cliniSocketWorker.Close();
                }

                if (cliniSocketListener.Connected)
                {
                    cliniSocketListener.Close();
                }
            }
        }
    }
}

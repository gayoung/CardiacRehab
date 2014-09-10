using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CardiacRehab
{
    class AsyncServer
    {
        private AsyncCallback socketWorkerCallback;
        private List<Socket> socket_list = new List<Socket>();
        private List<Socket> socketWorker_list = new List<Socket>();
        List<ContactInfo> PatientInfos;

        DoctorWindow currentwindow;

        /// <summary>
        /// class representation of the bio data of the patient as
        /// TCP packet
        /// </summary>
        class BioSocketPacket
        {
            public System.Net.Sockets.Socket packetSocket;
            public byte[] dataBuffer = new byte[666];
        }

        /// <summary>
        /// Keeps track of the state of each client connection
        /// </summary>
        class State
        {
            public int state_index = 0;
            public int port;
        }

        /// <summary>
        /// Keeps track of where the received data came from (ie. from which port)
        /// </summary>
        class ReceivedDataState
        {
            public BioSocketPacket socketData;
            public int port;
        }

        public AsyncServer(List<ContactInfo> list, DoctorWindow docwin)
        {
            PatientInfos = list;
            currentwindow = docwin;
        }
        // code for this section was modified from 
        // http://social.msdn.microsoft.com/Forums/en-US/f3151296-8064-4358-98a3-7ecf3d2c474b/multiple-ports-listening-on-c?forum=ncl

        public void StartListening()
        {
            int index = 0;
            foreach (ContactInfo patient in PatientInfos)
            {
                try
                {
                    State state = new State();
                    state.state_index = index++;
                    state.port = 5000 + patient.assigned_index-1;

                    //create listening socket
                    Socket currentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    currentSocket.NoDelay = true;
                    //Console.WriteLine("listening on :  " + localIP + ":" + portNum.ToString());
                    IPAddress addy = IPAddress.Parse(patient.address);
                    IPEndPoint iplocal = new IPEndPoint(addy, 5000 + patient.assigned_index);
                    //bind to local IP Address
                    currentSocket.Bind(iplocal);
                    //start listening -- 4 is max connections queue, can be changed
                    currentSocket.Listen(4);
                    //create call back for client connections -- aka maybe recieve video here????
                    currentSocket.BeginAccept(new AsyncCallback(OnConnection), state);
                    socket_list.Add(currentSocket);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("error at StartListening");
                    MessageBox.Show(e.Message);
                }
            }

        }

        /// <summary>
        /// Call back function that is invoked when the client is connected
        /// </summary>
        /// <param name="asyn"></param>
        private void OnConnection(IAsyncResult asyn)
        {
            var state = asyn.AsyncState as State;
            var port = state.port;
            var currentSocket = socket_list[state.state_index];

            try
            {
                currentSocket.NoDelay = true;
                socketWorker_list.Add(currentSocket.EndAccept(asyn));
                WaitForData(socketWorker_list[socketWorker_list.Count - 1], port);
                // start accepting connections from other clients
                currentSocket.BeginAccept(new AsyncCallback(OnConnection), state);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnConnection: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("error at OnConnection");
                MessageBox.Show(e.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("argument exception at OnBioSocketConnection");
                Console.WriteLine(ex.Message);
            }

        }
        private void WaitForData(System.Net.Sockets.Socket soc, int port)
        {
            try
            {
                if (socketWorkerCallback == null)
                {
                    socketWorkerCallback = new AsyncCallback(OnDataReceived);
                }

                ReceivedDataState state = new ReceivedDataState();
                BioSocketPacket sockpkt = new BioSocketPacket();
                state.socketData = sockpkt;
                state.port = port;

                soc.NoDelay = true;
                sockpkt.packetSocket = soc;
                //start listening for data
                soc.BeginReceive(sockpkt.dataBuffer, 0, sockpkt.dataBuffer.Length, SocketFlags.None, socketWorkerCallback, state);
            }
            catch (SocketException e)
            {
                Console.WriteLine("error at wait for biodata");
                MessageBox.Show(e.Message);
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                var state = asyn.AsyncState as ReceivedDataState;
                var socketData = state.socketData;
                var port = state.port;

                //end receive
                int end = 0;
                end = socketData.packetSocket.EndReceive(asyn);

                //just getting simple text right now -- needs to be changed
                char[] chars = new char[end + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int len = d.GetChars(socketData.dataBuffer, 0, end, chars, 0);
                String receivedData = new String(chars);
                receivedData = Regex.Replace(receivedData, @"\t|\n|\r", " ");

                currentwindow.processData(receivedData);
                WaitForData(socketData.packetSocket, state.port);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("OnDataReceived SocketException");
                MessageBox.Show(e.Message);
            }

        }

        
    }
}

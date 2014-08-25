using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardiacRehab
{
    /// <summary>
    /// class representation of the bio data of the patient as
    /// TCP packet
    /// </summary>
    class BioSocketPacket
    {
        public System.Net.Sockets.Socket packetSocket;
        public byte[] dataBuffer = new byte[200];
    }

    class BioSocket
    {
        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker = null;
        String IpAddress;
        int PortNumber;
        PatientWindow window;
        int patientindex;
        int patientDbId;
        int sessionId;

        public BioSocket(String ip, int port, int index, int dbId, int session, PatientWindow currentwindow)
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
                socketBioListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketBioListener.NoDelay = true;
                IPAddress addy = IPAddress.Parse(IpAddress);
                IPEndPoint iplocal = new IPEndPoint(addy, PortNumber);
                //bind to local IP Address
                socketBioListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                socketBioListener.Listen(4);
                //create call back for client connections -- aka maybe recieve video here????
                socketBioListener.BeginAccept(new AsyncCallback(OnBioSocketConnection), null);
            }
            catch (SocketException e)
            {
                //something went wrong
                Console.WriteLine("SocketException thrown at InitializeBiosockets");
                MessageBox.Show(e.Message);
            }

        }
        private void OnBioSocketConnection(IAsyncResult asyn)
        {
            try
            {
                bioSocketWorker = socketBioListener.EndAccept(asyn);
                bioSocketWorker.NoDelay = true;
                WaitForBioData(bioSocketWorker);
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\n OnSocketConnection: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at OnBioSocketConnection");
                MessageBox.Show(e.Message);
            }

        }
        private void WaitForBioData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (socketBioWorkerCallback == null)
                {
                    socketBioWorkerCallback = new AsyncCallback(OnBioDataReceived);
                }

                BioSocketPacket sockpkt = new BioSocketPacket();
                soc.NoDelay = true;
                sockpkt.packetSocket = soc;
                //start listening for data
                soc.BeginReceive(sockpkt.dataBuffer, 0, sockpkt.dataBuffer.Length, SocketFlags.None, socketBioWorkerCallback, sockpkt);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at WaitForBioData");
                MessageBox.Show(e.Message);
            }
        }

        private void OnBioDataReceived(IAsyncResult asyn)
        {
            try
            {
                BioSocketPacket socketID = (BioSocketPacket)asyn.AsyncState;
                //end receive
                int end = 0;
                end = socketID.packetSocket.EndReceive(asyn);

                //Console.WriteLine(end.ToString());

                // If the phone stops sending, then the EndReceive function returns 0
                // (i.e. zero bytes received)
                if (end == 0)
                {
                    socketID.packetSocket.Close();
                    socketBioListener.Close();
                    InitializeBioSockets();
                }
                // phone is connected!
                else
                {
                    char[] chars = new char[end + 1];
                    Decoder d = Encoding.UTF8.GetDecoder();
                    int len = d.GetChars(socketID.dataBuffer, 0, end, chars, 0);
                    String tmp = new String(chars);

                    window.ProcessBioSocketData(tmp, PortNumber);
                    InsertDataToDb(tmp);

                    WaitForBioData(bioSocketWorker);
                }
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException at OnBioDataReceived");
                // this error is thrown when the doctor disconnects
                // need to add code to close sockets and close the application
                MessageBox.Show(e.Message);
            }

        }

        private void InsertDataToDb(String data)
        {
            DatabaseClass db = new DatabaseClass();
            String[] datainfo = data.Trim().Split(' ');
            String fields = "";
            String tablename = "";
            String values = "";
            switch (PortNumber)
            {
                // !! ADD ENCRYPTION !! 

                // HR and OX
                case 4444:
                    if (datainfo[0] == "HR")
                    {
                        tablename = "hr_data";
                        fields = "heart_rate, session_id";
                        values = datainfo[1].Trim() + ", " + sessionId.ToString();
                    }
                    else if (datainfo[0] == "OX")
                    {
                        tablename = "ox_data";
                        fields = "oxy_sat, session_id";
                        values = datainfo[1].Trim() + ", " + sessionId.ToString();
                    }
                    else if(datainfo[0] == "UI")
                    {
                        tablename = "ui_data";
                        fields = "ui_value, session_id";
                        values = datainfo[1].Trim() + "," + sessionId.ToString();
                    }
                    break;
                // BP
                case 4445:
                    tablename = "bp_data";
                    fields = "systolic, diastolic, session_id";
                    values = datainfo[0].Trim() + ", " + datainfo[1].Trim() + ", " + sessionId.ToString();
                    break;
                // ECG
                case 4446:
                    tablename = "ecg_data";
                    fields = "ecg_data, session_id";
                    String processed = data.Substring(0, data.Length - 3);
                    values = "'"+processed.Trim() + "', " + sessionId.ToString();
                    break;
                // Bike
                case 4447:
                    if (datainfo[0] == "PW")
                    {
                        tablename = "power_data";
                        fields = "power_value, resistance_lv, session_id";
                        values = datainfo[1].Trim() + ", " + datainfo[2].Trim() + ", " + sessionId.ToString();
                    }
                    else if (datainfo[0] == "WR")
                    {
                        tablename = "wheel_data";
                        fields = "wheel_value, session_id";
                        values = datainfo[1].Trim() + ", " + sessionId.ToString();
                    }
                    else if (datainfo[0] == "CR")
                    {
                        tablename = "crank_data";
                        fields = "crank_value, session_id";
                        values = datainfo[1].Trim() + ", " + sessionId.ToString();
                    }
                    break;
            }

            if((tablename != "") && (fields != "") && (values != ""))
            {
                db.InsertRecord(tablename, fields, values);
            }
        }

        public void CloseSocket()
        {
            if(bioSocketWorker != null)
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

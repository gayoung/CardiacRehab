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

    class UdpBiosocket
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

        public UdpBiosocket(String ip, int port, int index, int dbId, int session, PatientWindow currentwindow)
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

                socketBioListener.BeginReceiveFrom(udpDataBuffer, 0, udpDataBuffer.Length, SocketFlags.None, ref iplocal, DoReceiveFrom, socketBioListener);
            }
            catch (SocketException e)
            {
                //something went wrong
                Console.WriteLine("SocketException thrown at InitializeBiosockets");
                MessageBox.Show(e.Message);
            }

        }

        private void DoReceiveFrom(IAsyncResult iar)
        {
            try
            {
                //Get the received message.
                //Console.WriteLine("DoReceiveFrom1");
                Socket recvSock = (Socket)iar.AsyncState;
                //Console.WriteLine("DoReceiveFrom2");
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                //Console.WriteLine("DoReceiveFrom3");
                int msgLen = recvSock.EndReceiveFrom(iar, ref clientEP);
                //Console.WriteLine("DoReceiveFrom4");
                byte[] localMsg = new byte[msgLen];
                //Console.WriteLine("DoReceiveFrom5");
                Array.Copy(udpDataBuffer, localMsg, msgLen);
                //Console.WriteLine("DoReceiveFrom6");

                //Start listening for a new message.
                IPAddress addy = IPAddress.Parse(IpAddress);
                //Console.WriteLine("DoReceiveFrom7");
                EndPoint iplocal = new IPEndPoint(addy, PortNumber);
                //Console.WriteLine("DoReceiveFrom8");
                socketBioListener.BeginReceiveFrom(udpDataBuffer, 0, udpDataBuffer.Length, SocketFlags.None, ref iplocal, DoReceiveFrom, socketBioListener);

                //Console.WriteLine("DoReceiveFrom9");
                if (msgLen == 0)
                {
                    Console.WriteLine("disconnected at " + PortNumber);
                    socketBioListener.Close();
                    InitializeBioSockets();
                }
                // phone is connected!
                else
                {
                    //Console.WriteLine("received at " + PortNumber);
                    char[] chars = new char[msgLen + 1];
                    Decoder d = Encoding.UTF8.GetDecoder();
                    int len = d.GetChars(udpDataBuffer, 0, msgLen, chars, 0);
                    String tmp = new String(chars);

                    //Console.WriteLine("received: " + tmp);

                    window.ProcessBioSocketData(tmp, PortNumber);
                    InsertDataToDb(tmp);
                }

                //Do other, more interesting, things with the received message.
            }
            catch (ObjectDisposedException)
            {
                //expected termination exception on a closed socket.
                // ...I'm open to suggestions on a better way of doing this.
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
                    else if (datainfo[0] == "UI")
                    {
                        tablename = "ui_data";
                        fields = "ui_value, session_id";
                        values = datainfo[1].Trim() + "," + sessionId.ToString();
                    }
                    else if (datainfo[0] == "BP")
                    {
                        // insert cloud data to db.
                        if (window.BpCloudData != "")
                        {
                            String bpData = window.BpCloudData;
                            String[] received = bpData.Split('/');

                            for (int i = 0; i < received.Length; i++)
                            {
                                if (received[i] != "")
                                {
                                    String[] bpdata = received[i].Split(',');
                                    tablename = "bp_data";
                                    fields = "systolic, diastolic, session_id";
                                    values = bpdata[0].Trim() + ", " + bpdata[1].Trim() + ", " + sessionId.ToString();
                                }
                            }
                        }
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
                    values = "'" + processed.Trim() + "', " + sessionId.ToString();
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

            if ((tablename != "") && (fields != "") && (values != ""))
            {
                db.InsertRecord(tablename, fields, values);
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

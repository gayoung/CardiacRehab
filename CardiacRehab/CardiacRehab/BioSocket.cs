﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
        public byte[] dataBuffer = new byte[1024];
    }

    class BioSocket
    {
        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker;
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
                    PostBioData(tmp);

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
                    values = data.Trim();
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

        // send HTTP POST
        private void PostBioData(String data)
        {
            String url = "";
            String[] datainfo = data.Trim().Split(' ');
            var postdata = new NameValueCollection();


            // later change this to get IP address of the DNS server
            switch (PortNumber)
            {
                case 4444:
                    url = "http://192.168.0.105/patients/" + patientindex.ToString() + "/biodata/others";
                    if(datainfo[0].Trim() == "HR")
                    {
                        postdata["hr"] = datainfo[1].Trim();
                    }
                    else if(datainfo[0].Trim() == "OX")
                    {
                        postdata["ox"] = datainfo[1].Trim();
                    }
                    break;
                case 4445:
                    url = "http://192.168.0.105/patients/" + patientindex.ToString() + "/biodata/bp";
                    postdata["bp"] = data.Trim();
                    break;
                case 4446:
                    url = "http://192.168.0.105/patients/" + patientindex.ToString() + "/biodata/ecg";
                    postdata["ecg"] = data.Trim();
                    break;
                case 4447:
                    url = "http://192.168.0.105/patients/" + patientindex.ToString() + "/biodata/bike";
                    if (datainfo[0].Trim() == "PW")
                    {
                        postdata["pw"] = datainfo[1].Trim();
                    }
                    else if (datainfo[0].Trim() == "WR")
                    {
                        postdata["wr"] = datainfo[1].Trim();
                    }
                    else if (datainfo[0].Trim() == "CR")
                    {
                        postdata["cr"] = datainfo[1].Trim();
                    }
                    break;
            }

            if(url != "")
            {
                using (var wb = new WebClient())
                {
                    var response = wb.UploadValues(url, "POST", postdata);
                }
            }
            else
            {
                Console.WriteLine("POST URL is not initialized");
            }
        }
    }
}

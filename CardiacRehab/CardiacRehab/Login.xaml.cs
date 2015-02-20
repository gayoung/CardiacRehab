﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        int sessionID = 0;
        int userid;
        String recordValues;
        String wirelessIP;
        String docID;
        String currentRole = "";
        String hostUrl = "http://192.168.0.101:5050/doctors/";
        int modeChosen;

        private DispatcherTimer checkForDocTimer;

        public Login()
        {
            InitializeComponent();
            GetLocalIP();
            FocusManager.SetFocusedElement(loginscreen, usernameinput);
        }

        public class TestIp
        {
            public String ipaddress;
        }

        /// <summary>
        /// This method is used to get both LAN and wireless IP of the current user
        /// </summary>
        private void GetLocalIP()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            int Ipcounter = 0;
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (Ipcounter == 0)
                    {
                        //patientLocalIp = addr.ToString();
                        wirelessIP = addr.ToString();
                    }
                    else if (Ipcounter == 1)
                    {
                        wirelessIP = addr.ToString();
                    }
                    Ipcounter++;
                }
            }
        }

        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            String username = usernameinput.Text.Trim();
            String password = passwordinput.Password.Trim();
            modeChosen = modeinput.SelectedIndex;

            // database connection and check login info
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("id, role", "authentication", "username='" + username + "' AND password='" + password + "'");
            HttpRequestClass postrequest = new HttpRequestClass();

            // found the user in the DB Record
            if (result[0].Count() > 0)
            {
                userid = int.Parse(result[0][0].Trim());

                if(result[1][0] == "Patient")
                {
                    currentRole = "Patient";

                    if (modeChosen == 0)
                    {
                        // get doctor DB ID
                        List<String>[] patientResult = db.SelectRecords("staff_id", "patient", "patient_id=" + userid);
                        docID = patientResult[0][0].Trim();

                        ContactInfo patientinfo = new ContactInfo();
                        patientinfo.id = userid;
                        patientinfo.name = username;
                        patientinfo.session = 0;
                        patientinfo.address = wirelessIP;
                        patientinfo.assigned_index = 0;

                        // post current patient's info
                        postrequest.PostContactInfo(hostUrl + docID + "/patients/" + userid.ToString() + "/", patientinfo);

                        warning_label.Content = "Waiting for the Clinician...";
                        warning_label.Visibility = System.Windows.Visibility.Visible;

                        InitTimer();

                    }
                    else
                    {
                        Console.WriteLine("offline");
                        List<String>[] offlinedoc = db.SelectRecords("staff_id", "clinical_staff", "fname='Offline' AND lname='Offline'");
                        docID = offlinedoc[0][0].Trim();

                        String recordValue = userid.ToString() + ",  " + docID + ", NOW(), 0";
                        sessionID = db.InsertRecord("patient_session", "patient_id, staff_id, date_start, chosen_level", recordValue);

                        PatientWindow patientWindow = new PatientWindow(1, userid, sessionID, "127.0.0.1", wirelessIP);
                        patientWindow.Show();
                        patientWindow.Closed += new EventHandler(MainWindowClosed);
                        this.Hide();
                    }
                    
                }
                else if(result[1][0] == "Doctor")
                {
                    currentRole = "Doctor";

                    ContactInfo doctorinfo = new ContactInfo();
                    doctorinfo.id = userid;
                    doctorinfo.name = "";
                    doctorinfo.session = 0;
                    doctorinfo.address = wirelessIP;
                    doctorinfo.assigned_index = 0;

                    postrequest.PostContactInfo(hostUrl + userid + "/", doctorinfo);

                    PatientList listwindow = new PatientList(userid);
                    listwindow.Show();
                    listwindow.Closed += new EventHandler(MainWindowClosed);
                    this.Hide();
                }
                else if(result[1][0] == "Admin")
                {
                    Console.WriteLine("authenticated user is a admin");
                }
            }
            else
            {
                warning_label.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // need this function to close the hidden login form
        private void MainWindowClosed(object sender, EventArgs e)
        {
            //Console.WriteLine("Closing loginwindow");
            this.Close();
        }

        /// <summary>
        /// This method is used to retrieve the ip address of the doctor from a designated URL.
        /// It uses the patient database ID to query the database ID of the clinician linked to the
        /// patient (i.e. the clinician that created this patient's account) and checks for the
        /// IP address of the clinician at the designated URL. (If the clinician has logged in,
        /// then the IP address will have been posted up on the URL)
        /// </summary>
        /// <param name="patientid"> Database ID of the patient</param>
        /// <returns> IP address of the clinician if it exists. Otherwise an empty string. </returns>
        private ContactInfo GetDoctorInfo()
        {
            // check for doctor IP
            HttpRequestClass getDoc = new HttpRequestClass();
            // later change the host name...
            Console.WriteLine(hostUrl);
            String docinfo = getDoc.GetPostData(hostUrl + docID + "/").Trim();

            ContactInfo docData = new ContactInfo();

            if (docinfo != "\"no data\"")
            {
                docData = JsonConvert.DeserializeObject<ContactInfo>(docinfo);
                Console.WriteLine(docData.address);
            }
            else
            {
                Console.WriteLine("no data was sent");
            }

            return docData;
        }

        private ContactInfo CheckSession()
        {
            HttpRequestClass getRequest = new HttpRequestClass();
            String patientinfo = getRequest.GetPostData(hostUrl + docID + "/patients/" + userid.ToString() + "/").Trim();

            ContactInfo patientData = new ContactInfo();
            if(patientinfo != "\"no data\"")
            {
                patientData = JsonConvert.DeserializeObject<ContactInfo>(patientinfo);
                return patientData;
            }
            else
            {
                return null;
            }
        }

        public void InitTimer()
        {
            checkForDocTimer = new System.Windows.Threading.DispatcherTimer();
            checkForDocTimer.Tick += new EventHandler(CheckForDocIP);
            checkForDocTimer.Interval = new TimeSpan(0, 0, 10); ; // 10 seconds
            checkForDocTimer.Start();
        }

        /// <summary>
        /// Function called by the timer class.
        /// This method is called every 10 seconds.
        /// *** EXPERIMENTAL CODE *** NOT TESTED!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForDocIP(object sender, EventArgs e)
        {
            ContactInfo patientInfo = CheckSession();

            if(patientInfo != null)
            {
                if (patientInfo.session != 0)
                {
                    ContactInfo currentdocInfo = GetDoctorInfo();
                    warning_label.Content = "Connected!";
                    warning_label.Visibility = System.Windows.Visibility.Visible;

                    sessionID = patientInfo.session;
                    int patientindex = patientInfo.assigned_index;
                    String doctorIP = currentdocInfo.address;

                    checkForDocTimer.Stop();

                    PatientWindow patientWindow = new PatientWindow(patientindex, userid, sessionID, doctorIP, wirelessIP);
                    patientWindow.Show();
                    patientWindow.Closed += new EventHandler(MainWindowClosed);
                    this.Hide();
                }
                else
                {
                    Console.WriteLine("session ID has not been updated. (i.e. doctor has not started the session)");
                }
            }
            else
            {
                Console.WriteLine("There is no patient information at the specified URL.");
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HttpRequestClass deleteRequest = new HttpRequestClass();
            // HTTP DELETE the posted data
            if(currentRole == "Patient")
            {
                deleteRequest.DeleteData(hostUrl + docID + "/patients/" + userid.ToString() + "/");

                // offline
                if(modeChosen == 1)
                {
                    DatabaseClass db = new DatabaseClass();
                    db.UpdateRecord("patient_session", "date_end=NOW()", "id=" + sessionID.ToString());
                }
            }
            else if(currentRole == "Doctor")
            {
                deleteRequest.DeleteData(hostUrl + userid + "/");
            }
        }

    }
}

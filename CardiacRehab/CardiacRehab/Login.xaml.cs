using Newtonsoft.Json;
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
        int chosenIndex;
        int userid;
        String recordValues;
        String wirelessIP;
        String docID;

        private DispatcherTimer mimicPhoneTimer;

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
            chosenIndex = indexinput.SelectedIndex;

            // database connection and check login info
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("id, role", "authentication", "username='" + username + "' AND password='" + password + "'");

            // found the user in the DB Record
            if (result[0].Count() > 0)
            {
                userid = int.Parse(result[0][0].Trim());

                // POST the user's information
                //HttpRequestClass postrequest = new HttpRequestClass();
                //postrequest.PostContactInfo(wirelessIP, username, result[1][0].Trim());
                //String webData = postrequest.GetPostData("http://192.168.0.105:5050/users/contacts/");

                // JSON --> Class
                //ContactInfo postdata = JsonConvert.DeserializeObject<ContactInfo>(webData);

                if(result[1][0] == "Patient")
                {
                    // get doctor DB ID
                    List<String>[] patientResult = db.SelectRecords("staff_id", "patient", "patient_id=" + userid);
                    docID = patientResult[0][0].Trim();

                    InitTimer();

                    // post current patient's info
                    HttpRequestClass postrequest = new HttpRequestClass();
                    postrequest.PostContactInfo("http://192.168.0.105:5050/doctors/" + docID + 
                        "/patients/" + userid.ToString() + "/", wirelessIP, username);

                    warning_label.Content = "Waiting for the Clinician...";
                    warning_label.Visibility = System.Windows.Visibility.Visible;
                }
                else if(result[1][0] == "Doctor")
                {
                    // POST current IP to doctor/<int:dbid>/
                    // change below code to open PatientList window
                    // (PatientList window will query the db for all patients under this doc & check for
                    // their data at doctors/<dbid>/patients/<dbid> )

                    recordValues = userid.ToString() + ",  NOW(), 0";
                    sessionID = db.InsertRecord("patient_session", "patient_id, date_start, chosen_level", recordValues);

                    DoctorWindow doctorWindow = new DoctorWindow(userid, sessionID);
                    doctorWindow.Show();
                    doctorWindow.Closed += new EventHandler(MainWindowClosed);
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
            Console.WriteLine("Closing loginwindow");
            DatabaseClass db = new DatabaseClass();
            db.UpdateRecord("patient_session", "date_end=NOW()", "id=" + sessionID.ToString());
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
            String docinfo = getDoc.GetPostData("http://192.168.0.105:5050/doctors/" + docID + "/").Trim();

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

        public void InitTimer()
        {
            mimicPhoneTimer = new System.Windows.Threading.DispatcherTimer();
            mimicPhoneTimer.Tick += new EventHandler(mimicPhoneTimer_Tick);
            mimicPhoneTimer.Interval = new TimeSpan(0, 0, 5); ; // 2 seconds
            mimicPhoneTimer.Start();
        }

        /// <summary>
        /// Function called by the timer class.
        /// This method is called every 10 seconds.
        /// *** EXPERIMENTAL CODE *** NOT TESTED!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mimicPhoneTimer_Tick(object sender, EventArgs e)
        {
            ContactInfo currentdocInfo = GetDoctorInfo();

            if(currentdocInfo.session != 0)
            {
                warning_label.Content = "Connected!";
                warning_label.Visibility = System.Windows.Visibility.Visible;

                sessionID = currentdocInfo.session;
                String doctorIP = currentdocInfo.address;

                mimicPhoneTimer.Stop();

                PatientWindow patientWindow = new PatientWindow(chosenIndex, userid, sessionID, doctorIP, wirelessIP);
                patientWindow.Show();
                patientWindow.Closed += new EventHandler(MainWindowClosed);
                this.Hide();
            }
        }
    }
}

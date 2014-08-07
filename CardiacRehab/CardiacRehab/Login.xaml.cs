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

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        int sessionID;
        String recordValues;
        String wirelessIP;
        String webData;

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
            int chosenIndex = indexinput.SelectedIndex;

            // database connection and check login info
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("id, role", "authentication", "username='" + username + "' AND password='" + password + "'");

            // found the user in the DB Record
            if (result[0].Count() > 0)
            {
                int userid = int.Parse(result[0][0].Trim());

                HttpRequestClass postrequest = new HttpRequestClass();
                postrequest.PostContactInfo(wirelessIP, username);
                webData = postrequest.GetPostData("http://192.168.0.105:5050/users/contacts/");

                Console.WriteLine("POST data from the URL: " + webData);
      
                if(result[1][0] == "Patient")
                {
                    Console.WriteLine("Patient Login");

                    recordValues = userid.ToString() + ",  NOW(), 0";
                    sessionID = db.InsertRecord("patient_session", "patient_id, date_start, chosen_level", recordValues);

                    PatientWindow patientWindow = new PatientWindow(chosenIndex, userid, sessionID);
                    patientWindow.Show();
                    patientWindow.Closed += new EventHandler(MainWindowClosed);
                    this.Hide();
                }
                else if(result[1][0] == "Doctor")
                {
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
    }
}

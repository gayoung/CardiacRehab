using System;
using System.Collections.Generic;
using System.Linq;
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

        public Login()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(loginscreen, usernameinput);
        }

        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            String username = usernameinput.Text.Trim();
            String password = passwordinput.Password.Trim();
            int chosenIndex = indexinput.SelectedIndex;

            // database connection and check login info
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("id, role", "authentication", "username='" + username + "' AND password='" + password + "'");

            if (result[0].Count() > 0)
            {
                int userid = int.Parse(result[0][0].Trim());
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

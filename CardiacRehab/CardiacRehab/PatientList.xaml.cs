using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for PatientList.xaml
    /// </summary>
    public partial class PatientList : Window
    {
        /// <summary>
        /// This window needs to:
        ///     - when loading: need to read the input from the select menu and list patients dynamically
        ///     1) query call patient under the supervising doctor
        ///     2) check the URL(../doctors/<dbID>/patients/<dbID>/) for all to see if they are online
        ///         (then update the list to exclude the online patients)
        ///     3) update the list accordingly
        /// </summary>

        //private int oldSelection = 5;
        //private int newSelection = 0;
        private int doctorId;
        private static int MAX_PATIENTS = 6;
        private DispatcherTimer getPatientTimer;
        private ContactInfo patientData;
        private List<ContactInfo> connected_patients = null;
        private String hosturl = "http://192.168.0.106:5050/doctors/";


        public PatientList(int dbId)
        {
            doctorId = dbId;
            connected_patients = new List<ContactInfo>();

            InitializeComponent();
            InitTimer();
        }

        public void InitTimer()
        {
            getPatientTimer = new System.Windows.Threading.DispatcherTimer();
            getPatientTimer.Tick += new EventHandler(GetConnecterdPatients);
            getPatientTimer.Interval = new TimeSpan(0, 0, 5); ; // 5 seconds
            getPatientTimer.Start();
        }

        private void GetConnecterdPatients(object sender, EventArgs e)
        {
            if (connected_patients != null)
            {
                connected_patients.Clear();
            }

            DatabaseClass db = new DatabaseClass();
            List<String>[] listOfPatients = db.SelectRecords("patient_id", "patient", "staff_id='" + doctorId.ToString() + "'");

            HttpRequestClass getRequest = new HttpRequestClass();

            // this function needs to be improved... it fails if connected patient disconnects while waiting for
            // the other patients to connect... it needs a better way to detect connected patients.
            String getData = "";
            for (int i = 0; i < listOfPatients[0].Count; i++)
            {
                getData = getRequest.GetPostData(hosturl + doctorId.ToString() + "/patients/" + listOfPatients[0][i] + "/");
                if (!getData.Contains("no data"))
                {
                    patientData = JsonConvert.DeserializeObject<ContactInfo>(getData);
                    connected_patients.Add(patientData);
                }
            }
            DisplayPatientList();
        }

        /// <summary>
        /// This method change the UI to reflect number of connected patients.
        /// </summary>
        private void DisplayPatientList()
        {
            int labelIndex = 1;
            for (int index = 0; index < connected_patients.Count; index++)
            {
                Label label = (Label)this.FindName("patient_status" + labelIndex.ToString());

                if (label != null)
                {
                    String content = label.Content.ToString();

                    if (content.Contains("Waiting for connection"))
                    {
                        label.Content = "Username: " + connected_patients.ElementAt(index).name + " Connected!";

                        ToggleCheckMark(labelIndex.ToString(), true);
                        AddPatientInfo(labelIndex.ToString(), connected_patients.ElementAt(index).address, connected_patients.ElementAt(index).id);
                    }
                }
                else
                {
                    Console.WriteLine("Label is null");
                    Console.WriteLine("Label:  patient_status" + index.ToString());
                }

                labelIndex++;
            }

        }

        /// <summary>
        /// This method is used to toggle the visibility of the
        /// checkmark image to indicate connected/disconnected patients.
        /// </summary>
        /// <param name="index">index number associated with the checkmark image</param>
        /// <param name="show">true if want image to be visible, otherwise false</param>
        private void ToggleCheckMark(String index, bool show)
        {
            Image checkMark = (Image)this.FindName("checkmark" + index);

            if (checkMark != null)
            {
                if (show)
                {
                    checkMark.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    checkMark.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                Console.WriteLine("checkmark image is null");
            }
        }

        /// <summary>
        /// Alter the content of hidden labels to store the patients'
        /// IP address and their database ID.
        /// </summary>
        /// <param name="index"> index associated with the hidden labels </param>
        /// <param name="ip"> IP address of the patient </param>
        /// <param name="id"> Database ID of the patient </param>
        private void AddPatientInfo(String index, String ip, int id)
        {
            Label ipAddress = (Label)this.FindName("patient_ip" + index);
            if (ipAddress != null)
            {
                ipAddress.Content = ip;
            }
            else
            {
                Console.WriteLine("hidden ip label is null");
            }

            Label idLabel = (Label)this.FindName("patient_id" + index);
            if (idLabel != null)
            {
                idLabel.Content = id.ToString();
            }
            else
            {
                Console.WriteLine("hidden id label is null");
            }
        }

        // start socket connections and video/audio connections with patients
        // and insert session record and update to doctor/doctorID url with ContactInfo.session
        private void start_session_Click(object sender, RoutedEventArgs e)
        {
            // check if all the patients are connected
            if (connected_patients.Count == 0)
            {
                String messageBoxText = "Please wait till at least one patient is connected.";
                String caption = "No patients";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else if (connected_patients.Count > 0)
            {
                bool allConnected = true;
                if (connected_patients.Count <= MAX_PATIENTS)
                {
                    allConnected = false;
                }

                if (!allConnected)
                {
                    // give dialog box to choose either to continue or to cancel
                    String messageBoxText = "Not all the patients have connected yet.\nAre you sure you want to start the session ?";
                    String caption = "Incomplete list of Patients";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    MessageBoxResult userInput = MessageBox.Show(messageBoxText, caption, button, icon);

                    if (userInput == MessageBoxResult.Yes)
                    {
                        getPatientTimer.Stop();
                        StartSession();
                    }
                }
                else
                {
                    getPatientTimer.Stop();
                    StartSession();
                }
            }
        }

        // insert records into patient_session and POST the session ID
        private void StartSession()
        {
            DatabaseClass db = new DatabaseClass();
            HttpRequestClass postReq = new HttpRequestClass();
            List<ContactInfo> finalList = new List<ContactInfo>();

            for (int index = 0; index < connected_patients.Count; index++)
            {
                ContactInfo patientInfo = connected_patients.ElementAt(index);
                patientInfo.assigned_index = index+1;

                String recordValue = patientInfo.id.ToString() + ",  " + doctorId.ToString() + ", NOW(), 0";
                int sessionID = db.InsertRecord("patient_session", "patient_id, staff_id, date_start, chosen_level", recordValue);
                
                patientInfo.session = sessionID;

                postReq.PostContactInfo(hosturl + doctorId.ToString() + "/patients/" + patientInfo.id.ToString() + "/", patientInfo);
                finalList.Add(patientInfo);
            }

            DoctorWindow docWindow = new DoctorWindow(finalList);
            docWindow.Show();
            docWindow.Closed += new EventHandler(DoctorWindowClosed);
            this.Hide();

        }

        private void DoctorWindowClosed(object sender, EventArgs e)
        {
            DatabaseClass db = new DatabaseClass();

            for(int index = 0; index < connected_patients.Count; index++)
            {
                ContactInfo patientInfo = connected_patients.ElementAt(index);
                db.UpdateRecord("patient_session", "date_end=NOW()", "id=" + patientInfo.session.ToString());
            }

            connected_patients.Clear();

            this.Close();
        }
    }
}

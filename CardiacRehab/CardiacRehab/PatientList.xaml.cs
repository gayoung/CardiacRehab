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

        private int oldSelection = 5;
        private int newSelection = 0;
        private int doctorId;
        private DispatcherTimer getPatientTimer;
        private ContactInfo patientData;
        private List<ContactInfo> connected_patients;
        private String hosturl = "http://192.168.0.102:5050/doctors/";


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
            DatabaseClass db = new DatabaseClass();
            List<String>[] listOfPatients = db.SelectRecords("patient_id", "patient", "staff_id='" + doctorId.ToString() + "'");

            HttpRequestClass getRequest = new HttpRequestClass();

            // this function needs to be improved... it fails if connected patient disconnects while waiting for
            // the other patients to connect... it needs a better way to detect connected patients.
            String getData = "";
            for(int i=0; i < listOfPatients[0].Count; i++)
            {
                getData = getRequest.GetPostData(hosturl + doctorId.ToString() + "/patients/" + listOfPatients[0][i] + "/");
                if(!getData.Contains("no data"))
                {
                    patientData = JsonConvert.DeserializeObject<ContactInfo>(getData);
                    Console.WriteLine(patientData.address);

                    connected_patients.Add(patientData);
                }
            }
            ResetPatientList();
            DisplayPatientList();
        }

        /// <summary>
        /// Reset the UI identical to when the application is launched.
        /// </summary>
        private void ResetPatientList()
        {
            for (int index = 1; index < newSelection+1; index++)
            {
                Label label = (Label)this.FindName("patient_status" + index.ToString());

                if (label != null)
                {
                    String content = label.Content.ToString();

                    if (!content.Contains("Waiting for connection"))
                    {
                        label.Content = "Waiting for connection";
                        ToggleCheckMark(index.ToString(), false);
                        AddPatientInfo(index.ToString(), "", 0);
                    }
                }
                else
                {
                    Console.WriteLine("Label is null");
                    Console.WriteLine("Label:  patient_status" + index.ToString());
                }
            }
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

            connected_patients.Clear();
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
                if(show)
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
            if(ipAddress != null)
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

        private void max_patients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<String> connected = new List<String>();
            // dynamically change the list of patients
            // (need to check for the label content before to see if the patient has already connected when the
            // doctor changed the number of patients)

            newSelection = max_patients.SelectedIndex;

            // find number of connected patients
           
            for(int i = 1; i < 7; i++)
            {
                Label label = (Label)this.FindName("patient_status" + i.ToString());
                if(label != null)
                {
                    String content = label.Content.ToString();

                    if (!content.Contains("Waiting for connection"))
                    {
                        connected.Add("patient_status" + i.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Label is null");
                    Console.WriteLine("Label:  patient_status" + i.ToString());
                }
            }

            if(connected.Count == 0)
            {
                connected = null;
            }

            if(connected != null)
            {
                if (connected.Count == newSelection+1)
                {
                    MessageBox.Show("GOT EVERYONE!");
                    TogglePatientList(oldSelection, newSelection, "none");
                }
                else if (connected.Count < newSelection+1)
                {
                    int difference = newSelection+1 - connected.Count;
                    MessageBox.Show("Missing " + difference.ToString() + " patients!");
                    TogglePatientList(oldSelection, newSelection, "none");
                }
                else
                {
                    MessageBox.Show("There are more people connected than maximum number of patients.");
                }
            }
            else
            {
                MessageBox.Show("No Patients are connected");
                TogglePatientList(oldSelection, newSelection, "none");
            }

            oldSelection = newSelection;
        }

        // This method changes the number of patients displayed as waiting on the
        // UI when the select menu is altered.
        private void TogglePatientList(int old, int selected, String connected)
        {
            // this condition might not be needed..
            if(connected == "none")
            {
                if(old > selected)
                {
                    int endNumber = selected + 1;
                    int diff = old - selected;

                    for (int i = 0; i < diff; i++)
                    {
                        endNumber++;

                        Rectangle rec = (Rectangle)this.FindName("patient_rec" + endNumber.ToString().Trim());
                        rec.Visibility = System.Windows.Visibility.Hidden;

                        Label patientStatus = (Label)this.FindName("patient_status" + endNumber.ToString());
                        patientStatus.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                // increased maximum
                else
                {
                    int endNumber = old + 1;
                    int diff = selected - old;

                    for (int i = 0; i < diff; i++)
                    {
                        endNumber++;

                        Rectangle rec = (Rectangle)this.FindName("patient_rec" + endNumber.ToString().Trim());
                        rec.Visibility = System.Windows.Visibility.Visible;

                        Label patientStatus = (Label)this.FindName("patient_status" + endNumber.ToString());
                        patientStatus.Visibility = System.Windows.Visibility.Visible;

                    }
                }
                
            }
            else
            {
                // there are some patients connected
                // If have to hide the rectangle with connected patient, give user an option to continue and drop the
                // existing connection with the patient or to cancel the selection
            }
        }

        // start socket connections and video/audio connections with patients
        // and insert session record and update to doctor/doctorID url with ContactInfo.session
        private void start_session_Click(object sender, RoutedEventArgs e)
        {
            // check if all the patients are connected
            if(connected_patients.Count == 0)
            {
                String messageBoxText = "Please wait till at least one patient is connected.";
                String caption = "No patients";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else if(connected_patients.Count > 0)
            {
                bool allConnected = true;
                if(connected_patients.Count < newSelection+1)
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

        // insert records into patient_session and launch doctorWindow
        private void StartSession()
        {
            DatabaseClass db = new DatabaseClass();
           for(int index = 0; index < connected_patients.Count; index++)
           {
               ContactInfo patientInfo = connected_patients.ElementAt(index);
               Console.WriteLine("Patient information " + index.ToString());
               Console.WriteLine(patientInfo.id);
               Console.WriteLine(patientInfo.address);
               Console.WriteLine(patientInfo.name);
               Console.WriteLine("End of Patient data");

           }
        }
    }
}

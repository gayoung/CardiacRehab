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
            getPatientTimer.Tick += new EventHandler(GetPatients);
            getPatientTimer.Interval = new TimeSpan(0, 0, 5); ; // 10 seconds
            getPatientTimer.Start();
        }

        private void GetPatients(object sender, EventArgs e)
        {
            DatabaseClass db = new DatabaseClass();
            List<String>[] listOfPatients = db.SelectRecords("patient_id", "patient", "staff_id='" + doctorId.ToString() + "'");

            HttpRequestClass getRequest = new HttpRequestClass();

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

            int labelIndex = 1;
            for(int index=0; index < connected_patients.Count; index++)
            {
                Label label = (Label)this.FindName("patient_status" + labelIndex.ToString());

                if (label != null)
                {
                    String content = label.Content.ToString();

                    if (content.Contains("Waiting for connection"))
                    {
                        label.Content = "Username: " + connected_patients.ElementAt(index).name + " Connected!";
                        
                        ToggleCheckMark(labelIndex.ToString());
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

        private void ToggleCheckMark(String index)
        {
            Image checkMark = (Image)this.FindName("checkmark" + index);

            if (checkMark != null)
            {
                checkMark.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Console.WriteLine("checkmark image is null");
            }
        }

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
                    MessageBox.Show("Something went wrong!");
                }
            }
            else
            {
                MessageBox.Show("No Patients are connected");
                TogglePatientList(oldSelection, newSelection, "none");
            }

            oldSelection = newSelection;
        }

        private void TogglePatientList(int old, int selected, String connected)
        {
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
            Console.WriteLine("start button pressed");
        }
    }
}

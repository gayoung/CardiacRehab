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

        int oldSelection = 5;
        int newSelection = 0;


        public PatientList()
        {
            InitializeComponent();

        }

        private void max_patients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // dynamically change the list of patients
            // (need to check for the label content before to see if the patient has already connected when the
            // doctor changed the number of patients)

            newSelection = max_patients.SelectedIndex;

            // find number of connected patients
            String[] connected = null;
            int connected_index = 0;
            for(int i = 1; i < 7; i++)
            {
                Label label = (Label)this.FindName("patient_status" + i.ToString());
                if(label != null)
                {
                    String content = label.Content.ToString();

                    if (!content.Contains("Waiting for connection"))
                    {
                        connected[connected_index] = "patient_status" + i.ToString();
                        connected_index++;
                    }
                }
                else
                {
                    Console.WriteLine("Label is null");
                    Console.WriteLine("Label:  patient_status" + i.ToString());
                }
            }

            if(connected != null)
            {
                if (connected.Length == newSelection)
                {
                    MessageBox.Show("GOT EVERYONE!");
                }
                else if (connected.Length < newSelection)
                {
                    int difference = newSelection - connected.Length;
                    MessageBox.Show("Missing " + difference.ToString() + " patients!");
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
            Console.WriteLine("old: " + old.ToString());
            Console.WriteLine("selected: " + selected.ToString());

            if(connected == "none")
            {
                if(old > selected)
                {
                    int endNumber = selected + 1;
                    int diff = old - selected;

                    for (int i = 0; i < diff; i++)
                    {
                        endNumber++;

                        Console.WriteLine("Looking for: patient_rec" + endNumber.ToString());

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

                        Console.WriteLine("Looking for: patient_rec" + endNumber.ToString());

                        Rectangle rec = (Rectangle)this.FindName("patient_rec" + endNumber.ToString().Trim());
                        rec.Visibility = System.Windows.Visibility.Visible;

                        Label patientStatus = (Label)this.FindName("patient_status" + endNumber.ToString());
                        patientStatus.Visibility = System.Windows.Visibility.Visible;

                    }
                }
                
            }
            else
            {

            }
        }
    }
}

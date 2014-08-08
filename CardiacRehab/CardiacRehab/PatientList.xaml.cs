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
         

        public PatientList()
        {
            InitializeComponent();

        }

        private void max_patients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // dynamically change the list of patients
            // (need to check for the label content before to see if the patient has already connected when the
            // doctor changed the number of patients)

            int maxNum = max_patients.SelectedIndex;

            // find number of connected patients
            String[] connected = null;
            int connected_index = 0;
            for(int i = 1; i < 7; i++)
            {
                Label label = (Label)this.FindName("patient_status" + i.ToString());
                String content = label.Content.ToString();

                if(!content.Contains("Waiting for connection"))
                {
                    connected[connected_index] = "patient_status" + i.ToString();
                    connected_index++;
                }
            }

            if(connected.Length == maxNum)
            {
                MessageBox.Show("GOT EVERYONE!");
            }
            else if(connected.Length < maxNum)
            {
                int difference = maxNum - connected.Length;
                MessageBox.Show("Missing " + difference.ToString() + " patients!");
            }
            else
            {
                MessageBox.Show("Something went wrong!");
            }
        }
    }
}

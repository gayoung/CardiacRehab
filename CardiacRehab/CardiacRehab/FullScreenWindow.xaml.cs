using DynamicDataDisplaySample.ECGViewModel;
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
using System.Windows.Threading;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for FullScreenWindow.xaml
    /// </summary>
    public partial class FullScreenWindow : Window
    {
        public int patientLabel;

        public ECGPointCollection ecgPointCollection;
        DispatcherTimer updateCollectionTimer = null;
        private double xaxisValue = 0;

        private DoctorWindow currentSplitScreen;

        public FullScreenWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the collapse button is triggered.
        /// It switches the view from the current FullScreenWindow object to the object of
        /// DoctorWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            // when this method is changed to Close then it closes the entire app...
            currentSplitScreen.ecgPointCollection = ecgPointCollection;
            ecgPointCollection = null;
            currentSplitScreen.Show();
            currentSplitScreen = null;
            GC.Collect();
            this.Close();

        }

        /// <summary>
        /// This method is called when the note button is triggered.  It triggers the
        /// PopupWindow to be open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteButton_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.PatientLabel.Content = "Patient " + patientLabel.ToString();
            popup.NoteTime.Content = DateTime.Now.ToString("HH:mm:ss");
            popup.ShowDialog();
        }

        /// <summary>
        /// This method is called when the mute button is triggered.  It mutes/unmutes
        /// the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            currentSplitScreen.ToggleMuteIcon(muteIcon);
        }
    }
}

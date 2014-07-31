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

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        private FullScreenWindow fullscreenview = null;
        private int userid;
        public ECGPointCollection ecgPointCollection;

        public DoctorWindow()
        {
            InitializeComponent();
        }

        private void DoctorWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// This method is called when the memo buttons are triggered and it 
        /// calls the PopupWindow UI that allows the user to leave notes
        /// about the selected patient during the session.
        /// </summary>
        /// <param name="index"> the index associated with the memo icon (which is associated with patients 1-6)</param>
        private void CreateMemoPopup(int index)
        {
            PopupWindow popup = new PopupWindow();
            popup.PatientLabel.Content = "Patient " + index.ToString();
            popup.NoteTime.Content = DateTime.Now.ToString("HH:mm:ss");
            popup.ShowDialog();
        }

        /// <summary>
        /// This method is called when the mute/unmute icons are clicked.  It
        /// togglees the icon images and also mute/unmute the audio for the selected
        /// patient.
        /// </summary>
        /// <param name="icon"> the selected muteIcon object </param>
        public void ToggleMuteIcon(System.Windows.Controls.Image icon)
        {
            String currentIcon = icon.Source.ToString();
            if (currentIcon.Contains("mute.png"))
            {
                icon.BeginInit();
                icon.Source = new BitmapImage(new Uri("mic.png", UriKind.RelativeOrAbsolute));
                icon.EndInit();
                //wo.Volume = oldVolume;
                // add code to enable volume again
            }
            else
            {
                icon.BeginInit();
                icon.Source = new BitmapImage(new Uri("mute.png", UriKind.RelativeOrAbsolute));
                icon.EndInit();
                //oldVolume = wo.Volume;
                //wo.Volume = 0f;
                // add code to mute the patient
            }
        }

        /// <summary>
        /// This method is triggered by expand button and it calls the FullScreenWindow object
        /// to display the selected patient view only. (instead of all 6 patients)
        /// </summary>
        /// <param name="patient"></param>
        private void ExpandedScreenView(int patient)
        {
            fullscreenview = new FullScreenWindow();
            this.Hide();
            fullscreenview.Show();
            fullscreenview.Closed += new EventHandler(ShowDoctorScreen);
        }

        /// <summary>
        /// Close the doctor window when the user triggers to close the
        /// application in fullscreen mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDoctorScreen(object sender, EventArgs e)
        {
            this.Show();
        }

        private void connect1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void connect2_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Memo Button Triggers

        private void memo1_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(1);
        }
        private void memo2_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(2);
        }
        private void memo3_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(3);
        }
        private void memo4_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(4);
        }
        private void memo5_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(5);
        }
        private void memo6_Click(object sender, RoutedEventArgs e)
        {
            CreateMemoPopup(6);
        }

        #endregion

        #region Mute Button Triggers

        private void mute1_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon1);
        }
        private void mute2_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon2);
        }
        private void mute3_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon3);
        }
        private void mute4_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon4);
        }
        private void mute5_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon5);
        }
        private void mute6_Click(object sender, RoutedEventArgs e)
        {
            ToggleMuteIcon(muteIcon6);
        }

        #endregion

        #region Expand Button Triggers

        private void expand1_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(1);
        }
        private void expand2_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(2);
        }
        private void expand3_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(3);
        }
        private void expand4_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(4);
        }
        private void expand5_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(5);
        }
        private void expand6_Click(object sender, RoutedEventArgs e)
        {
            ExpandedScreenView(6);
        }


        #endregion

    }
}

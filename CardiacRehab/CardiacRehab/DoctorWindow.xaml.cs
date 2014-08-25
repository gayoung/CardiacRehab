using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.Listeners;
using Coding4Fun.Kinect.KinectService.WpfClient;
using DynamicDataDisplaySample.ECGViewModel;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using ColorImageFormat = Microsoft.Kinect.ColorImageFormat;
using ColorImageFrame = Microsoft.Kinect.ColorImageFrame;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        List<ContactInfo> PatientList;

        WaveOut wo = new WaveOut();
        WaveFormat wf = new WaveFormat(16000, 1);
        BufferedWaveProvider mybufferwp = null;
        public float oldVolume;

        private FullScreenWindow fullscreenview = null;
        bool[] warningStatus = new bool[6];

        public DoctorWindow(List<ContactInfo> list)
        {
            PatientList = list;

            for(int index=0; index < PatientList.Count; index++)
            {
                ContactInfo patientinfo = PatientList.ElementAt(index);
                Console.WriteLine(patientinfo.address);
            }

            InitializeComponent();
            //userid = currentuser;
            //sessionID = session;

            //GetLocalIP();
            //InitializeComponent();

            //// patients send the biodata from port 5000-5005
            //int[] ports = new int[6] { 5000, 5001, 5002, 5003, 5004, 5005 };
            //InitializeBioSockets(ports);

        }

        private void DoctorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //int[] kinectOutPorts = new int[6] { 4531, 4532, 4533, 4534, 4535, 4536 };
            //InitializeKinect(kinectOutPorts);
            //InitializeAudio();

            //InitializeECG();
            ////InitTimer();

            //this.DataContext = this;
        }

        #region Helper functions


        /// <summary>
        /// This method is called when the memo buttons are triggered and it 
        /// calls the PopupWindow UI that allows the user to leave notes
        /// about the selected patient during the session.
        /// </summary>
        /// <param name="index"> the index associated with the memo icon (which is associated with patients 1-6)</param>
        private void CreateMemoPopup(int index)
        {
            ContactInfo patientinfo = PatientList.ElementAt(index);
            PopupWindow popup = new PopupWindow(patientinfo.session);
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
                wo.Volume = oldVolume;
                // add code to enable volume again
            }
            else
            {
                icon.BeginInit();
                icon.Source = new BitmapImage(new Uri("mute.png", UriKind.RelativeOrAbsolute));
                icon.EndInit();
                oldVolume = wo.Volume;
                wo.Volume = 0f;
                // add code to mute the patient
            }
        }

        /// <summary>
        /// This method toggles the uparrow.png and downarrow.png to let the doctor know the
        /// patients' biodata status.  If the heart rate is too high (> 80% of maximum HR) then
        /// the heart rate value is displayed as red color with the uparrow.png visible.
        /// (similar concept for oxygen sat and bp.)
        /// </summary>
        /// <param name="icon"> the image object placeholder in XAML file </param>
        /// <param name="currentLabel"> the label object in XAML file displaying the biodata value </param>
        /// <param name="newimg"> the name of the image file to be displayed </param>
        private void SetArrow(System.Windows.Controls.Image icon, Label currentLabel, String newimg)
        {
            //if the biodata is too low, then the font is displayed as blue.
            if (newimg == "Resources/downarrow.png")
            {
                currentLabel.Foreground = System.Windows.Media.Brushes.Blue;
            }
            //if the biodata is too high, then the font is displayed as blue.
            else
            {
                currentLabel.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
            // display the speciied images
            icon.BeginInit();
            icon.Source = new BitmapImage(new Uri(newimg, UriKind.RelativeOrAbsolute));
            icon.EndInit();
            icon.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This method is triggered by expand button and it calls the FullScreenWindow object
        /// to display the selected patient view only. (instead of all 6 patients)
        /// </summary>
        /// <param name="patient"></param>
        private void ExpandedScreenView(int patient)
        {
            //fullscreenview = new FullScreenWindow(userid, patient, this);
            //this.Hide();
            //fullscreenview.Show();
            //fullscreenview.Closed += new EventHandler(ShowDoctorScreen);
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


        /// <summary>
        /// This method is used to put the warning message about status of
        /// other patients when the doctor is in Full screen view mode of one patient.
        /// </summary>
        private void RaiseOtherPatientWarning()
        {
            int currentPatient = fullscreenview.patientLabel;
            String patientString = "";
            int warningindex = 1;

            while (warningindex < warningStatus.Length)
            {
                if ((warningStatus[warningindex - 1]) && (warningindex != currentPatient))
                {
                    patientString += "patient" + warningindex.ToString() + ", ";
                }
                warningindex++;
            }

            if (warningStatus[warningStatus.Length - 1])
            {
                patientString += "patient" + warningStatus.Length.ToString();
            }
            if (patientString != "")
            {
                fullscreenview.WarningLabel.Content = patientString + " need to be checked.";
                fullscreenview.WarningLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                fullscreenview.WarningLabel.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        #endregion


        #region Connect button triggers
        private void connect1_Click(object sender, RoutedEventArgs e)
        {
        }

        private void connect2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void InitializeAudio()
        {
            wo.DesiredLatency = 100;
            mybufferwp = new BufferedWaveProvider(wf);
            mybufferwp.BufferDuration = TimeSpan.FromMinutes(5);
            wo.Init(mybufferwp);
            wo.Play();
        }

        #endregion

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

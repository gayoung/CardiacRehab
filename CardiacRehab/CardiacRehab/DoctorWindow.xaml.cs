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

        String wirelessIP;

        // all TCP sockets getting biometric data from the patients
        ClinicianSockets patient1hrox;
        ClinicianSockets patient1uibp;
        ClinicianSockets patient1ecg;
        ClinicianSockets patient1bike;

        ClinicianSockets patient2hrox;
        ClinicianSockets patient2uibp;
        ClinicianSockets patient2ecg;
        ClinicianSockets patient2bike;

        ClinicianSockets patient3hrox;
        ClinicianSockets patient3uibp;
        ClinicianSockets patient3ecg;
        ClinicianSockets patient3bike;

        ClinicianSockets patient4hrox;
        ClinicianSockets patient4uibp;
        ClinicianSockets patient4ecg;
        ClinicianSockets patient4bike;

        ClinicianSockets patient5hrox;
        ClinicianSockets patient5uibp;
        ClinicianSockets patient5ecg;
        ClinicianSockets patient5bike;

        ClinicianSockets patient6hrox;
        ClinicianSockets patient6uibp;
        ClinicianSockets patient6ecg;
        ClinicianSockets patient6bike;

        WaveOut wo = new WaveOut();
        WaveFormat wf = new WaveFormat(16000, 1);
        BufferedWaveProvider mybufferwp = null;
        public float oldVolume;

        private FullScreenWindow fullscreenview = null;
        bool[] warningStatus = new bool[6];

        public DoctorWindow(List<ContactInfo> list)
        {
            PatientList = list;

            InitializeComponent();
            GetLocalIP();
            InitializeAllPatientSockets();
        }

        private void InitializeAllPatientSockets()
        {
            // Really need to refactor!!!
            patient1hrox = new ClinicianSockets(wirelessIP, 5001, this);
            patient1hrox.InitializeCliniSockets();
            patient1uibp = new ClinicianSockets(wirelessIP, 5002, this);
            patient1uibp.InitializeCliniSockets();
            patient1ecg = new ClinicianSockets(wirelessIP, 5003, this);
            patient1ecg.InitializeCliniSockets();
            patient1bike = new ClinicianSockets(wirelessIP, 5004, this);
            patient1bike.InitializeCliniSockets();

            //patient2hrox = new ClinicianSockets(wirelessIP, 5011, this);
            //patient2hrox.InitializeCliniSockets();
            //patient2uibp = new ClinicianSockets(wirelessIP, 5012, this);
            //patient2uibp.InitializeCliniSockets();
            //patient2ecg = new ClinicianSockets(wirelessIP, 5013, this);
            //patient2ecg.InitializeCliniSockets();
            //patient2bike = new ClinicianSockets(wirelessIP, 5014, this);
            //patient2bike.InitializeCliniSockets();

            //patient3hrox = new ClinicianSockets(wirelessIP, 5021, this);
            //patient3hrox.InitializeCliniSockets();
            //patient3uibp = new ClinicianSockets(wirelessIP, 5022, this);
            //patient3uibp.InitializeCliniSockets();
            //patient3ecg = new ClinicianSockets(wirelessIP, 5023, this);
            //patient3ecg.InitializeCliniSockets();
            //patient3bike = new ClinicianSockets(wirelessIP, 5024, this);
            //patient3bike.InitializeCliniSockets();

            //patient4hrox = new ClinicianSockets(wirelessIP, 5031, this);
            //patient4hrox.InitializeCliniSockets();
            //patient4uibp = new ClinicianSockets(wirelessIP, 5032, this);
            //patient4uibp.InitializeCliniSockets();
            //patient4ecg = new ClinicianSockets(wirelessIP, 5033, this);
            //patient4ecg.InitializeCliniSockets();
            //patient4bike = new ClinicianSockets(wirelessIP, 5034, this);
            //patient4bike.InitializeCliniSockets();

            //patient5hrox = new ClinicianSockets(wirelessIP, 5041, this);
            //patient5hrox.InitializeCliniSockets();
            //patient5uibp = new ClinicianSockets(wirelessIP, 5042, this);
            //patient5uibp.InitializeCliniSockets();
            //patient5ecg = new ClinicianSockets(wirelessIP, 5043, this);
            //patient5ecg.InitializeCliniSockets();
            //patient5bike = new ClinicianSockets(wirelessIP, 5044, this);
            //patient5bike.InitializeCliniSockets();

            //patient6hrox = new ClinicianSockets(wirelessIP, 5051, this);
            //patient6hrox.InitializeCliniSockets();
            //patient6uibp = new ClinicianSockets(wirelessIP, 5052, this);
            //patient6uibp.InitializeCliniSockets();
            //patient6ecg = new ClinicianSockets(wirelessIP, 5053, this);
            //patient6ecg.InitializeCliniSockets();
            //patient6bike = new ClinicianSockets(wirelessIP, 5054, this);
            //patient6bike.InitializeCliniSockets();
        }

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

        #region function to handle all received data sent from each of the patients

        public void UpdateUI(String data, int portNum)
        {
            // test code
            Console.WriteLine(data);
            String[] processedString = data.Trim().Split(' ');
            String header = processedString[0].Trim();

            switch (portNum)
            {
                case 5001:  // patient 1 heart rate + oxygen sat
                    if (header == "HR")
                    {
                        UpdatePatientHR(processedString[1], hrValue1);
                    }
                    else if (header == "OX")
                    {
                        UpdatePatientOX(processedString[1], oxiValue1);
                    }
                    break;
                case 5002:  // patient 1 intensity + blood pressure
                    if(header == "UI")
                    {
                        UpdatePatientUI(processedString[1], uiValue1);
                    }
                    else if(header == "BP")
                    {
                        UpdatePatientBP(processedString[1], processedString[2], bpSysValue1, bpDiaValue1);
                    }
                    break;
                case 5003:  // patient 1 ECG
                    // graph ECG
                    break;
                case 5004:  // patient 1 bike data
                    // not sure what to do with this info yet?
                    if(header == "PW")
                    {
                        Console.WriteLine(header);
                    }
                    else if(header == "WR")
                    {
                        Console.WriteLine(header);
                    }
                    else if(header == "CR")
                    {
                        Console.WriteLine(header);
                    }
                    break;
                case 5011:
                    break;
                case 5012:
                    break;
                case 5013:
                    break;
                case 5014:
                    break;
                case 5021:
                    break;
                case 5022:
                    break;
                case 5023:
                    break;
                case 5024:
                    break;
                case 5031:
                    break;
                case 5032:
                    break;
                case 5033:
                    break;
                case 5034:
                    break;
                case 5041:
                    break;
                case 5042:
                    break;
                case 5043:
                    break;
                case 5044:
                    break;
                case 5051:
                    break;
                case 5052:
                    break;
                case 5053:
                    break;
                case 5054:
                    break;

            }

            // need to update the UI to display the new values

            // later need to add methods to give clinicians warning
        }

        private void UpdatePatientHR(String hrdata, Label hrValLabel)
        {
            hrValLabel.Content = hrdata.Trim();
        }

        private void UpdatePatientOX(String oxdata, Label oxValLabel)
        {
            oxValLabel.Content = oxdata.Trim();
        }

        private void UpdatePatientUI(String uidata, Label uiValLabel)
        {
            uiValLabel.Content = uidata.Trim();
        }

        private void UpdatePatientBP(String systolic, String diastolic, Label sysLabel, Label diaLabel)
        {
            sysLabel.Content = systolic.Trim();
            diaLabel.Content = diastolic.Trim();
        }
        #endregion

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



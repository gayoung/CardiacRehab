using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.Listeners;
using Coding4Fun.Kinect.KinectService.WpfClient;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Threading;

using ColorImageFormat = Microsoft.Kinect.ColorImageFormat;
using ColorImageFrame = Microsoft.Kinect.ColorImageFrame;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for PatientWindow.xaml
    /// </summary>
    public partial class PatientWindow : Window
    {
        private int user;
        private int sessionID;
        // currently under assumption that
        // first output from the loop is LAN and second is wireless
        private String doctorIp;
        private String wirelessIP;

        private int patientIndex;
        private DispatcherTimer mimicPhoneTimer;

        public Socket socketToClinician = null;

        //kinect sensor 
        private KinectSensorChooser sensorChooser;

        //kinect listeners
        private static ColorListener _videoListener;
        private static AudioListener _audioListener;

        //kinect clients
        private ColorClient _videoClient;

        private WriteableBitmap outputImage;
        private byte[] pixels = new byte[0];

        private AudioClient _audioClient;

        WaveOut wo = new WaveOut();
        WaveFormat wf = new WaveFormat(16000, 1);
        BufferedWaveProvider mybufferwp = null;

        TextWriter _writer;

        UnitySocket unityBikeSocket = null;
        UnitySocket turnSocket = null;

        BioSocket otherSocket;
        BioSocket bpSocket;
        BioSocket ecgSocket;
        BioSocket bikeSocket;

        PhidgetEncoder rotary_encoder;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="currentuser"> database ID for the current user</param>
        public PatientWindow(int chosen, int currentuser, int session, String docIP, String wireless)
        {
            user = currentuser;
            patientIndex = chosen + 1;
            sessionID = session;
            doctorIp = docIP;
            wirelessIP = wireless;
            InitializeComponent();

            _writer = new TextBoxStreamWriter(txtMessage);
            Console.SetOut(_writer);

            unityBikeSocket = new UnitySocket(5555);
            unityBikeSocket.ConnectToUnity();

            InitializeVR();

            rotary_encoder = new PhidgetEncoder(3);

            //CreateSocketConnection();

            // later will have different port for different devices 
            otherSocket = new BioSocket(wirelessIP, 4444, patientIndex, currentuser, sessionID, this);
            otherSocket.InitializeBioSockets();

            bpSocket = new BioSocket(wirelessIP, 4445, patientIndex, currentuser, sessionID, this);
            bpSocket.InitializeBioSockets();

            ecgSocket = new BioSocket(wirelessIP, 4446, patientIndex, currentuser, sessionID, this);
            ecgSocket.InitializeBioSockets();

            bikeSocket = new BioSocket(wirelessIP, 4447, patientIndex, currentuser, sessionID, this);
            bikeSocket.InitializeBioSockets();
            

            // disable this function if InitializeBioSockets function is active
            InitTimer();
        }

        private void PatientWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
            //InitializeKinect();
            //InitializeAudio();

        }


        #region Helper functions

        private void InitializeVR()
        {
            //String debugpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //String projectpath = debugpath.Replace("\\bin\\Debug\\CardioRehab-WPF.exe", "");

            //Console.WriteLine(projectpath);

            // make this path relative later
            try
            {
                UnityWindow.Navigate("C:\\Users\\Gayoung\\Documents\\KdaysDemo\\web\\web.html");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error at InitializeVR");
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// This method calls mimicPhoneTimer_Tick method which calls the PhoneTestMethod
        /// to mimic the data sent by the phone at port 4444.
        /// 
        /// Used to test the application without having access to 6 phones to mock 6 proper patients.
        /// 
        /// The code was modified from
        /// http://stackoverflow.com/questions/6169288/execute-specified-function-every-x-seconds
        /// </summary>
        public void InitTimer()
        {
            mimicPhoneTimer = new System.Windows.Threading.DispatcherTimer();
            mimicPhoneTimer.Tick += new EventHandler(mimicPhoneTimer_Tick);
            mimicPhoneTimer.Interval = new TimeSpan(0, 0, 2); ; // 2 seconds
            mimicPhoneTimer.Start();
        }

        /// <summary>
        /// Function called by the timer class.
        /// 
        /// This method is called every 2 seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mimicPhoneTimer_Tick(object sender, EventArgs e)
        {
            PhoneTestMethod();
        }

        /// <summary>
        /// method to be used to test the code without the phone
        /// </summary>
        private void PhoneTestMethod()
        {
            String data;
            byte[] dataToClinician;
            byte[] dataToUnity;

            Random r = new Random();
            int heartRate = r.Next(60, 200);
            int oxygen = r.Next(93, 99);
            int systolic = r.Next(100, 180);
            int diastolic = r.Next(50, 120);

            // testing for bike data (values may not be in correct range)
            int powerVal = r.Next(20, 40);
            // should be between 100-200 (changed for faster testing)
            int speedVal = r.Next(150, 200);
            int cadenceVal = r.Next(40, 60);

            // modify patient UI labels
            hrValue.Dispatcher.Invoke((Action)(() => hrValue.Content = heartRate.ToString() + " bpm"));
            oxiValue.Dispatcher.Invoke((Action)(() => oxiValue.Content = oxygen.ToString() + " %"));
            bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = systolic.ToString() + "/" + diastolic.ToString()));

            String patientLabel = "patient" + patientIndex;

            try
            {
                //// mock data sent to the clinician
                //data = patientLabel + "-" + user.ToString() + "|" + "HR " + heartRate.ToString() + "\n";
                //dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                //socketToClinician.Send(dataToClinician);

                //data = patientLabel + "-" + user.ToString() + "|" + "OX " + oxygen.ToString() + "\n";
                //dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                //socketToClinician.Send(dataToClinician);

                //data = patientLabel + "-" + user.ToString() + "|" + "BP " + systolic.ToString() + " " + diastolic.ToString() + "\n";
                //dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                //socketToClinician.Send(dataToClinician);

                //data = patientLabel + "-" + user.ToString() + "|" + "EC -592 -201 -133 -173 -172 -143 -372 -349 -336 -332 -314 -309 -295 -274 -265 -261 16 44 75 102 -123 -80 -44 -11 259\n";
                //dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                //socketToClinician.Send(dataToClinician);

                if (unityBikeSocket.unitySocketWorker != null)
                {
                    if (unityBikeSocket.unitySocketWorker.Connected)
                    {
                        // mock data sent to the Unity Application
                        data = "PW " + powerVal.ToString() + "\n";
                        dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                        unityBikeSocket.unitySocketWorker.Send(dataToUnity);

                        data = "";

                        data = "WR " + speedVal.ToString() + "\n";
                        dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                        unityBikeSocket.unitySocketWorker.Send(dataToUnity);

                        data = "";

                        data = "CR " + cadenceVal.ToString() + "\n";
                        dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                        unityBikeSocket.unitySocketWorker.Send(dataToUnity);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        #region Setting up the socket connection for bio information

        public void ProcessBioSocketData(String tmp, int socketPortNumber)
        {
            System.String[] data = tmp.Trim().Split(' ');

            if (socketPortNumber == 4447)
            {
                if (unityBikeSocket.unitySocketWorker != null)
                {
                    if (unityBikeSocket.unitySocketWorker.Connected)
                    {
                        byte[] dataToUnity = System.Text.Encoding.ASCII.GetBytes(tmp);
                        unityBikeSocket.unitySocketWorker.Send(dataToUnity);
                    }
                }
            }

            // Decide on what encouragement text should be displayed based on heart rate.
            if (socketPortNumber == 4444)
            {
                if (data[0] == "HR")
                {
                    //BT
                    int number;
                    bool result = Int32.TryParse(data[1], out number);
                    if (result)
                    {
                        // remove null char
                        hrValue.Dispatcher.Invoke((Action)(() => hrValue.Content = data[1].Replace("\0", "").Trim() + " bpm"));
                    }

                }

                // Change the Sats display in the UI thread.
                if (data[0] == "OX")
                {
                    if (data.Length > 1)
                    {
                        // MethodInvoker had to be used to solve cross threading issue
                        if (data[1] != null && data[2] != null)
                        {
                            oxiValue.Dispatcher.Invoke((Action)(() => oxiValue.Content = data[1] + " %"));
                            // enable below to display hr from oximeter
                            hrValue.Dispatcher.Invoke((Action)(() => hrValue.Content = data[2].Replace("\0", "").Trim() + " bpm"));
                        }
                    }
                }
            }
            else if (socketPortNumber == 4445)
            {
                bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = data[1] + "/" + data[2]));
            }
        }
        #endregion

        #region socket connection with the doctor

        private void CreateSocketConnection()
        {
            try
            {
                //create a new client socket
                socketToClinician = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (doctorIp != null)
                {
                    System.Net.IPAddress remoteIPAddy = System.Net.IPAddress.Parse(doctorIp);
                    System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddy, 5000 + patientIndex - 1);
                    socketToClinician.Connect(remoteEndPoint);
                }
                else
                {
                    MessageBox.Show("doctor IP is null");
                }

            }

            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at CreateSocketConnection: " + e.ErrorCode.ToString());
                MessageBox.Show(e.Message);
            }
        }

        #endregion

        #region Kinect
        private void InitializeKinect()
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Don't try this unless there is a kinect
            if (this.sensorChooser.Kinect != null)
            {
                //trying to get the video from the clinician -- this can fail
                _videoClient = new ColorClient();
                _videoClient.ColorFrameReady += _videoClient_ColorFrameReady;
                _videoClient.Connect(doctorIp, 4531 + patientIndex - 1);


                // Streaming video out on port 4555
                _videoListener = new ColorListener(this.sensorChooser.Kinect, 4555 + patientIndex - 1, ImageFormat.Jpeg);
                _videoListener.Start();

                //_audioClient = new AudioClient();
                //_audioClient.AudioFrameReady += _audioClient_AudioFrameReady;
                //_audioClient.Connect(doctorIp, 4541 + patientIndex - 1);

                ////for sending audio
                //_audioListener = new AudioListener(this.sensorChooser.Kinect, 4565 + patientIndex - 1);
                //_audioListener.Start();

            }

        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arguments</param>
        void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            if (e.OldSensor != null)
            {
                try
                {
                    e.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.

                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    e.NewSensor.ColorFrameReady += NewSensor_ColorFrameReady;

                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }


        void NewSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                {
                    return;
                }

                if (pixels.Length == 0)
                {
                    this.pixels = new byte[frame.PixelDataLength];
                }
                frame.CopyPixelDataTo(this.pixels);

                outputImage = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

                outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

                this.patientFrame.Source = outputImage;

                // force the garbase collector to remove outputImage --> otherwise, causes mem leak
                outputImage = null;
                GC.Collect();
            };

        }

        //called when a video frame from the client is ready
        void _videoClient_ColorFrameReady(object sender, ColorFrameReadyEventArgs e)
        {
            this.doctorFrame.Source = e.ColorFrame.BitmapImage;
        }

        private void InitializeAudio()
        {
            wo.DesiredLatency = 100;
            mybufferwp = new BufferedWaveProvider(wf);
            mybufferwp.BufferDuration = TimeSpan.FromMinutes(5);
            wo.Init(mybufferwp);
            wo.Play();
        }

        void _audioClient_AudioFrameReady(object sender, AudioFrameReadyEventArgs e)
        {
            if (mybufferwp != null)
            {
                mybufferwp.AddSamples(e.AudioFrame.AudioData, 0, e.AudioFrame.AudioData.Length);
            }
        }


        #endregion
    }

}

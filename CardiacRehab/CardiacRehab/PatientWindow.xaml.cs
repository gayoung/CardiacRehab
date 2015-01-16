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
using System.Threading;
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

        // Timers for testing purposes
        private DispatcherTimer mimicPhoneTimer;
        private DispatcherTimer mimicBPTimer;

        // Timer to pull blood pressure data from the iHealth Cloud
        private DispatcherTimer BPTimer;
        //Socket socketToClinician = null;

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

        UdpBiosocket hrUdpSocket;
        UdpBiosocket bpUdpSocket;
        UdpBiosocket ecgUdpSocket;
        UdpBiosocket bikeUdpSocket;

        Socket HrOxToClinician = null;
        Socket UiBpToClinician = null;
        Socket EcgToClinician = null;
        Socket BikeToClinician = null;

        PhidgetEncoder rotary_encoder;
        IHealthClass ihealth;
        public String BpCloudData;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="currentuser"> database ID for the current user</param>
        public PatientWindow(int chosen, int currentuser, int session, String docIP, String wireless)
        {
            user = currentuser;
            patientIndex = chosen;
            sessionID = session;
            doctorIp = docIP;
            wirelessIP = wireless;
            InitializeComponent();
            StartApplication();
        }

        private void PatientWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //InitializeKinect();
            //InitializeAudio();
        }

        private void StartApplication()
        {
            // textbox in the UI for testing purposes
            _writer = new TextBoxStreamWriter(txtMessage);
            Console.SetOut(_writer);

            //ihealth = new IHealthClass(patientIndex, this);
            //ihealth.GetCode();

            unityBikeSocket = new UnitySocket(5555);
            unityBikeSocket.ConnectToUnity();

            //turnSocket = new UnitySocket(5556);
            //turnSocket.ConnectToUnity();

            InitializeVR();

            //rotary_encoder = new PhidgetEncoder(3, this);
            //rotary_encoder.Initialize();

            ConnectToDoctor();

            // later will have different port for different devices 
            //Console.WriteLine("initializing 4444");
            //otherSocket = new BioSocket(wirelessIP, 4444, patientIndex, user, sessionID, this);
            ////Thread otherThread = new Thread(new ThreadStart(otherSocket.InitializeBioSockets));
            ////otherThread.Start();
            //otherSocket.InitializeBioSockets();

            //Console.WriteLine("initializing 4445");
            //bpSocket = new BioSocket(wirelessIP, 4445, patientIndex, user, sessionID, this);
            ////Thread bpThread = new Thread(new ThreadStart(bpSocket.InitializeBioSockets));
            ////bpThread.Start();
            //bpSocket.InitializeBioSockets();

            //Console.WriteLine("initializing 4446");
            //ecgSocket = new BioSocket(wirelessIP, 4446, patientIndex, user, sessionID, this);
            ////Thread ecgThread = new Thread(new ThreadStart(ecgSocket.InitializeBioSockets));
            ////ecgThread.Start();
            //ecgSocket.InitializeBioSockets();

            //Console.WriteLine("initializing 4447");
            //bikeSocket = new BioSocket(wirelessIP, 4447, patientIndex, user, sessionID, this);
            ////Thread bikeThread = new Thread(new ThreadStart(bikeSocket.InitializeBioSockets));
            ////bikeThread.Start();
            //bikeSocket.InitializeBioSockets();
            //Console.WriteLine ("DONE");

            // Working for biodata + bike with UDP
            //Console.WriteLine("connecting to: " + wirelessIP);
            //hrUdpSocket = new UdpBiosocket(wirelessIP, 4444, patientIndex, user, sessionID, this);
            //hrUdpSocket.InitializeBioSockets();
            //bpUdpSocket = new UdpBiosocket(wirelessIP, 4445, patientIndex, user, sessionID, this);
            //bpUdpSocket.InitializeBioSockets();
            //ecgUdpSocket = new UdpBiosocket(wirelessIP, 4446, patientIndex, user, sessionID, this);
            //ecgUdpSocket.InitializeBioSockets();
            //bikeUdpSocket = new UdpBiosocket(wirelessIP, 4447, patientIndex, user, sessionID, this);
            //bikeUdpSocket.InitializeBioSockets();


            // Disable this function if testing with InitTimer()
            //InitMockBPTimer();
            InitTimer();
        }

        #region VR code
        private void InitializeVR()
        {
            String debugpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            String projectpath = debugpath.Replace("\\CardiacRehab\\CardiacRehab\\bin\\Debug\\CardiacRehab.exe", "");
            projectpath = projectpath + "\\BikeVR\\BikeVR.html";

            // make this path relative later
            try
            {
                UnityWindow.Navigate(projectpath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error at InitializeVR");
                Console.WriteLine(e.Message);
            }

        }

        public void ProcessEncoderData(String encoderData)
        {
            if (turnSocket.unitySocketWorker != null)
            {
                if (turnSocket.unitySocketWorker.Connected)
                {
                    // indicates if the rotation was CCW (+) or CW (-)
                    Byte[] dataToUnity = System.Text.Encoding.ASCII.GetBytes(encoderData);
                    turnSocket.unitySocketWorker.Send(dataToUnity);
                }
            }
        }
        #endregion

        //#region blood Pressure Test code

        // ****************************** TEST CODE **************************

        //public void InitMockBPTimer()
        //{
        //    mimicBPTimer = new System.Windows.Threading.DispatcherTimer();
        //    mimicBPTimer.Tick += new EventHandler(mimicBP_timer);
        //    mimicBPTimer.Interval = new TimeSpan(0, 0, 20); ; // 3 min
        //    mimicBPTimer.Start();
        //}

        ///// <summary>
        ///// Function called by the timer class.
        ///// 
        ///// This method is called every 2 seconds.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void mimicBP_timer(object sender, EventArgs e)
        //{
        //    BPPhoneTest();
        //}

        //private void BPPhoneTest()
        //{
        //    if (ihealth.Access_token != null)
        //    {
        //        DateTime startUnix = DateTime.Now.AddMinutes(-3);
        //        String startTime = UnixTime.ToUnixTime(startUnix).ToString();

        //        String endTime = UnixTime.ToUnixTime(DateTime.Now).ToString();
        //        String bloodPressureData = ihealth.GetBloodPressure(startTime, endTime);

        //        if (bloodPressureData != "")
        //        {
        //            String[] received = bloodPressureData.Split('/');


        //            for (int i = 0; i < received.Length; i++)
        //            {
        //                if (received[i] != "")
        //                {
        //                    String[] bpdata = received[i].Split(',');
        //                    bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = bpdata[0] + " / " + bpdata[1]));

        //                    String data;
        //                    byte[] dataToClinician;

        //                    data = "BP " + bpdata[0] + " " + bpdata[1] + "\n";
        //                    dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
        //                    UiBpToClinician.Send(dataToClinician);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Something happened with connection with iHealth Cloud");
        //    }

        //}
        //#endregion

        //#region mimicking phone code
        //// *******************************************************************

        ///// <summary>
        ///// This method calls mimicPhoneTimer_Tick method which calls the PhoneTestMethod
        ///// to mimic the data sent by the phone at port 4444.
        ///// 
        ///// Used to test the application without having access to 6 phones to mock 6 proper patients.
        ///// 
        ///// The code was modified from
        ///// http://stackoverflow.com/questions/6169288/execute-specified-function-every-x-seconds
        ///// </summary>
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
            int intensityVal = r.Next(6, 20);

            // testing for bike data (values may not be in correct range)
            int powerVal = r.Next(20, 40);
            // should be between 100-200 (changed for faster testing)
            int speedVal = r.Next(150, 200);
            int cadenceVal = r.Next(40, 60);

            // modify patient UI labels
            hrValue.Dispatcher.Invoke((Action)(() => hrValue.Content = heartRate.ToString() + " bpm"));
            oxiValue.Dispatcher.Invoke((Action)(() => oxiValue.Content = oxygen.ToString() + " %"));
            bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = systolic.ToString() + "/" + diastolic.ToString()));

            try
            {
                // mock data sent to the clinician
                data = "HR " + heartRate.ToString() + "   ";
                dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                HrOxToClinician.Send(dataToClinician);

                data = "OX " + oxygen.ToString() + "   ";
                dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                HrOxToClinician.Send(dataToClinician);

                data = "BP " + systolic.ToString() + " " + diastolic.ToString() + "   ";
                dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                UiBpToClinician.Send(dataToClinician);

                data = "UI " + intensityVal.ToString() + "    ";
                dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                UiBpToClinician.Send(dataToClinician);

                data = "-592 -201 -133 -173 -172 -143 -372 -349 -336 -332 -314 -309 -295 -274 -265 -261 16 44 75 102 -123 -80 -44 -11 259   ";
                dataToClinician = System.Text.Encoding.ASCII.GetBytes(data);
                EcgToClinician.Send(dataToClinician);

                data = "PW " + powerVal.ToString() + "   ";
                dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                BikeToClinician.Send(dataToUnity);

                data = "";

                data = "WR " + speedVal.ToString() + "   ";
                dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                BikeToClinician.Send(dataToUnity);

                data = "";

                data = "CR " + cadenceVal.ToString() + "   ";
                dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                BikeToClinician.Send(dataToUnity);
                if (unityBikeSocket.unitySocketWorker != null)
                {
                    if (unityBikeSocket.unitySocketWorker.Connected)
                    {
                        try
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
                        catch (SocketException e)
                        {
                            Console.WriteLine("timer socket exception: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        //#endregion

        //#region Process data sent from the phone

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
                            //hrValue.Dispatcher.Invoke((Action)(() => hrValue.Content = data[2].Replace("\0", "").Trim() + " bpm"));
                        }
                    }
                }
                //if (data[0] == "PB")
                //{
                //    // get data from the cloud
                //    // Once prompted by the phone, pull every 30 seconds 
                //    // for any data taken a minute from current time until
                //    // new data is detected. (currently just checks if
                //    // there has been a measurement in last one minute)
                //    InitBPTimer();
                //}
            }
            else if (socketPortNumber == 4445)
            {
                bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = data[0] + "/" + data[1]));
            }
        }

        //private void InitBPTimer()
        //{
        //    BPTimer = new System.Windows.Threading.DispatcherTimer();
        //    BPTimer.Tick += new EventHandler(BPTimerMethod);
        //    BPTimer.Interval = new TimeSpan(0, 0, 30); ; // 3 min
        //    BPTimer.Start();
        //}

        ///// <summary>
        ///// Function called by the timer class.
        ///// 
        ///// This method is called every 2 seconds.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void BPTimerMethod(object sender, EventArgs e)
        //{
        //    PullBpFromCloud();
        //}

        //private void PullBpFromCloud()
        //{
        //    if (ihealth.Access_token != null)
        //    {
        //        DateTime startUnix = DateTime.Now.AddMinutes(-1);
        //        String startTime = UnixTime.ToUnixTime(startUnix).ToString();

        //        String endTime = UnixTime.ToUnixTime(DateTime.Now).ToString();
        //        BpCloudData = ihealth.GetBloodPressure(startTime, endTime);

        //        if (BpCloudData != "")
        //        {
        //            String[] received = BpCloudData.Split('/');


        //            for (int i = 0; i < received.Length; i++)
        //            {
        //                if (received[i] != "")
        //                {
        //                    // got the value so update UI and stop timer
        //                    String[] bpdata = received[i].Split(',');
        //                    bpValue.Dispatcher.Invoke((Action)(() => bpValue.Content = bpdata[0] + " / " + bpdata[1]));
        //                    BPTimer.Stop();
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Something happened with connection with iHealth Cloud");
        //    }

        //}
        //#endregion

        //#region socket connection with the doctor

        private void ConnectToDoctor()
        {
            try
            {
                int indexNumber = 5001 + (patientIndex - 1) * 10;
                HrOxToClinician = CreateClinicianSocket(indexNumber);
                indexNumber++;
                UiBpToClinician = CreateClinicianSocket(indexNumber);
                indexNumber++;
                EcgToClinician = CreateClinicianSocket(indexNumber);
                indexNumber++;
                BikeToClinician = CreateClinicianSocket(indexNumber);
            }

            catch (SocketException e)
            {
                Console.WriteLine("SocketException thrown at CreateSocketConnection: " + e.ErrorCode.ToString());
                MessageBox.Show(e.Message);
            }
        }

        private Socket CreateClinicianSocket(int port)
        {
            Socket newsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (doctorIp != null)
            {
                newsocket.NoDelay = true;
                System.Net.IPAddress remoteIPAddy = System.Net.IPAddress.Parse(doctorIp);
                System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddy, port);
                newsocket.Connect(remoteEndPoint);
            }
            else
            {
                MessageBox.Show("doctor IP is null");
                newsocket = null;
            }
            return newsocket;
        }

        //#endregion

        //#region Kinect
        //private void InitializeKinect()
        //{
        //    this.sensorChooser = new KinectSensorChooser();
        //    this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
        //    this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
        //    this.sensorChooser.Start();

        //    // Don't try this unless there is a kinect
        //    if (this.sensorChooser.Kinect != null)
        //    {
        //        //trying to get the video from the clinician -- this can fail
        //        _videoClient = new ColorClient();
        //        _videoClient.ColorFrameReady += _videoClient_ColorFrameReady;
        //        _videoClient.Connect(doctorIp, 4531 + patientIndex - 1);


        //        // Streaming video out on port 4555
        //        _videoListener = new ColorListener(this.sensorChooser.Kinect, 4555 + patientIndex - 1, ImageFormat.Jpeg);
        //        _videoListener.Start();

        //        //_audioClient = new AudioClient();
        //        //_audioClient.AudioFrameReady += _audioClient_AudioFrameReady;
        //        //_audioClient.Connect(doctorIp, 4541 + patientIndex - 1);

        //        ////for sending audio
        //        //_audioListener = new AudioListener(this.sensorChooser.Kinect, 4565 + patientIndex - 1);
        //        //_audioListener.Start();

        //    }

        //}

        ///// <summary>
        ///// Called when the KinectSensorChooser gets a new sensor
        ///// </summary>
        ///// <param name="sender">sender of the event</param>
        ///// <param name="e">event arguments</param>
        //void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        //{
        //    if (e.OldSensor != null)
        //    {
        //        try
        //        {
        //            e.OldSensor.ColorStream.Disable();
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
        //            // E.g.: sensor might be abruptly unplugged.

        //        }
        //    }

        //    if (e.NewSensor != null)
        //    {
        //        try
        //        {
        //            e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
        //            e.NewSensor.ColorFrameReady += NewSensor_ColorFrameReady;

        //        }
        //        catch (InvalidOperationException)
        //        {
        //            // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
        //            // E.g.: sensor might be abruptly unplugged.
        //        }
        //    }
        //}


        //void NewSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        //{
        //    using (ColorImageFrame frame = e.OpenColorImageFrame())
        //    {
        //        if (frame == null)
        //        {
        //            return;
        //        }

        //        if (pixels.Length == 0)
        //        {
        //            this.pixels = new byte[frame.PixelDataLength];
        //        }
        //        frame.CopyPixelDataTo(this.pixels);

        //        outputImage = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

        //        outputImage.WritePixels(
        //            new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

        //        this.patientFrame.Source = outputImage;

        //        // force the garbase collector to remove outputImage --> otherwise, causes mem leak
        //        outputImage = null;
        //        GC.Collect();
        //    };

        //}

        ////called when a video frame from the client is ready
        //void _videoClient_ColorFrameReady(object sender, ColorFrameReadyEventArgs e)
        //{
        //    this.doctorFrame.Source = e.ColorFrame.BitmapImage;
        //}

        //private void InitializeAudio()
        //{
        //    wo.DesiredLatency = 100;
        //    mybufferwp = new BufferedWaveProvider(wf);
        //    mybufferwp.BufferDuration = TimeSpan.FromMinutes(5);
        //    wo.Init(mybufferwp);
        //    wo.Play();
        //}

        //void _audioClient_AudioFrameReady(object sender, AudioFrameReadyEventArgs e)
        //{
        //    if (mybufferwp != null)
        //    {
        //        mybufferwp.AddSamples(e.AudioFrame.AudioData, 0, e.AudioFrame.AudioData.Length);
        //    }
        //}


        //#endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //    // clean up 
            //    rotary_encoder.CloseEncoder();
            //    unityBikeSocket.CloseSocket();
            //    turnSocket.CloseSocket();
            //    otherSocket.CloseSocket();
            //    bpSocket.CloseSocket();
            //    ecgSocket.CloseSocket();
            //    bikeSocket.CloseSocket();

            //    if (mybufferwp != null)
            //    {
            //        mybufferwp.ClearBuffer();

            //        wo.Stop();
            //        wo.Dispose();
            //    }
        }
    }

}

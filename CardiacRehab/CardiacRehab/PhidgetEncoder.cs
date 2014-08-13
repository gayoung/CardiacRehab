using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets;
using Phidgets.Events;

namespace CardiacRehab
{
    class PhidgetEncoder
    {
        Phidgets.Encoder encoder;
        int serialNumber = 0;
        UnitySocket turnSocket;
        String positionChange;
        String time;
        String encoderPosition;
        String data;
        Byte[] dataToUnity;

        public PhidgetEncoder()
        {
            encoder = new Phidgets.Encoder();

            encoder.Attach += new AttachEventHandler(encoder_Attach);
            encoder.Detach += new DetachEventHandler(encoder_Detach);
            encoder.Error += new ErrorEventHandler(encoder_Error);
            encoder.PositionChange += new EncoderPositionChangeEventHandler(encoder_PositionChange);
            encoder.open();
            encoder.waitForAttachment();

            turnSocket = new UnitySocket(5556);
            turnSocket.ConnectToUnity();
        }

        public void CloseEncoder()
        {
            //Unhook the event handlers
            encoder.Attach -= new AttachEventHandler(encoder_Attach);
            encoder.Detach -= new DetachEventHandler(encoder_Detach);
            encoder.Error -= new ErrorEventHandler(encoder_Error);
            encoder.PositionChange -= new EncoderPositionChangeEventHandler(encoder_PositionChange);

            encoder.close();
        }
        
        // this method is mostly for debugging purposes (to see the encoder attached)
        // This method is mostly used for debugging purposes.
        void encoder_Attach(object sender, AttachEventArgs e)
        {
            Console.WriteLine("encoder attached");
            Phidgets.Encoder attached = (Phidgets.Encoder)sender;
            Console.WriteLine(attached.Attached.ToString());
            Console.WriteLine(attached.Name);
            Console.WriteLine(attached.Version.ToString());
            Console.WriteLine(attached.encoders.Count.ToString());
            Console.WriteLine(attached.inputs.Count.ToString());
        }

        //Detach event code...We'll clear our display fields and disable our editable fields while device is not attached
        //as trying to communicate with the device while not attached will generate a PhidgetException.  In this example,
        //I have coded so that this should not occur, but best practice would be to catch it and handle it accordingly
        void encoder_Detach(object sender, DetachEventArgs e)
        {
            Console.WriteLine("Encoder detached");
        }

        //Error event handler...we'll just send the description text to a popup message box for this example
        void encoder_Error(object sender, ErrorEventArgs e)
        {
            Phidget phid = (Phidget)sender;

            Console.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + e.Description);
        }

        //Encoder Position Change event handler...the event arguements will provide the encoder index, value, and 
        //the elapsed time since the last event.  These value, including the current position value stored in the
        //corresponding element in the encoder objects encoder collection could be used to calculate velocity...
        void encoder_PositionChange(object sender, EncoderPositionChangeEventArgs e)
        {
            positionChange = e.PositionChange.ToString();
            Console.WriteLine("Position Change: " + positionChange);

            try
            {
                time = e.Time.ToString();
                Console.WriteLine("Time: " + time);
            }
            catch
            {
                time = "Unknown";
                Console.WriteLine("Unknown");
            }

            encoderPosition = encoder.encoders[e.Index].ToString();
            Console.WriteLine("Encoder Position: " + encoderPosition);

            if(turnSocket.unitySocketWorker != null)
            {
                if(turnSocket.unitySocketWorker.Connected)
                {
                    data = "PC " + positionChange + "\n";
                    dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                    turnSocket.unitySocketWorker.Send(dataToUnity);

                    data = "Time " + time + "\n";
                    dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                    turnSocket.unitySocketWorker.Send(dataToUnity);

                    data = "EP " + encoderPosition + "\n";
                    dataToUnity = System.Text.Encoding.ASCII.GetBytes(data);
                    turnSocket.unitySocketWorker.Send(dataToUnity);
                }
            }

        }

    }
}

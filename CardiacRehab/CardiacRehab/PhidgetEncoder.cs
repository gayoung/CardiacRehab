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

        public PhidgetEncoder()
        {
            encoder = new Phidgets.Encoder();

            encoder.Attach += new AttachEventHandler(encoder_Attach);
            encoder.Detach += new DetachEventHandler(encoder_Detach);
            encoder.Error += new ErrorEventHandler(encoder_Error);
            encoder.PositionChange += new EncoderPositionChangeEventHandler(encoder_PositionChange);
            
            encoder.open();
            encoder.waitForAttachment();
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

        void encoder_Attach(object sender, AttachEventArgs e)
        {
            Console.WriteLine("encoder attached");
            Phidgets.Encoder attached = (Phidgets.Encoder)sender;
            Console.WriteLine(attached.Attached.ToString());
            Console.WriteLine(attached.Name);
            Console.WriteLine(attached.SerialNumber.ToString());
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
            Console.WriteLine("Position Change: "+e.PositionChange.ToString());
            try
            {
                Console.WriteLine("Time: "+e.Time.ToString());
            }
            catch
            {
                Console.WriteLine("Unknown");
            }

            Console.WriteLine("Encoder Position: "+encoder.encoders[e.Index].ToString());

        }

    }
}

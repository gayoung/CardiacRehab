using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CardiacRehab
{
    class HttpRequestClass
    {
            private int heartrate;
            private int oxygen;
            private int systolic;
            private int diastolic;
            private String ecg;
            private int power;
            private int wheelrev;
            private int crankrev;

        public void PostBioData(String data, int PortNumber, int patientindex)
        {
            String url = "";
            String[] datainfo = data.Trim().Split(' ');
            url = "http://192.168.0.105:5050/patients/" + patientindex.ToString() + "/biodata/";

            // later change this to get IP address of the DNS server
            switch (PortNumber)
            {
                case 4444:
                    if (datainfo[0].Trim() == "HR")
                    {
                        this.heartrate = Convert.ToInt32(datainfo[1].Trim());
                    }
                    else if (datainfo[0].Trim() == "OX")
                    {
                        this.oxygen = Convert.ToInt32(datainfo[1].Trim());
                    }
                    break;
                case 4445:
                    this.systolic = Convert.ToInt32(datainfo[0].Trim());
                    this.diastolic = Convert.ToInt32(datainfo[1].Trim());
                    break;
                case 4446:
                    this.ecg = data.Trim();
                    break;
                case 4447:
                    // bike data comes in floats
                    String[] woFloatPoint = datainfo[1].Trim().Split('.');
                    if (datainfo[0].Trim() == "PW")
                    {
                        this.power = Convert.ToInt32(woFloatPoint[0].Trim());
                    }
                    else if (datainfo[0].Trim() == "WR")
                    {
                        this.wheelrev = Convert.ToInt32(woFloatPoint[0].Trim());
                    }
                    else if (datainfo[0].Trim() == "CR")
                    {
                        this.crankrev = Convert.ToInt32(woFloatPoint[0].Trim());
                    }
                    break;
            }

            if (url != "")
            {
                //using (var wb = new WebClient())
                //{
                //    var response = wb.UploadValues(url, "POST", postdata);
                //}

                String jsonData = JsonConvert.SerializeObject(this);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                ASCIIEncoding encoding = new ASCIIEncoding();
                request.Method = "POST";
                request.ContentType = "text/json";
                request.ContentLength = jsonData.Length;
                byte[] bytedata = encoding.GetBytes(jsonData);

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytedata, 0, bytedata.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responsestring = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            else
            {
                Console.WriteLine("POST URL is not initialized");
            }
        }
    }
}

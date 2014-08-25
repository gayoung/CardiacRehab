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

    public class ContactInfo
    {
        public String name;
        public String address;
        public int id;
        public int assigned_index;
        public int session = 0; // DB ID of the session
    }

    class HttpRequestClass
    {
        // add session
        public void PostContactInfo(String url, ContactInfo contact)
        {
            String jsonData = JsonConvert.SerializeObject(contact);

            Console.WriteLine("requested URL: " + url);

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
        }

        public String GetPostData(String url)
        {
            byte[] getData = new WebClient().DownloadData(url);

            String receivedData = Encoding.UTF8.GetString(getData);

            return receivedData;
        }

        public void DeleteData(String url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine("HTTP response: " + response.StatusCode.ToString());
        }

    }
}

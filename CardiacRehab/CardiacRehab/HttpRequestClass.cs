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
        public String ipAddress;
        public String name;
    }

    class HttpRequestClass
    {
        public void PostContactInfo(String ip, String username)
        {
            ContactInfo contact = new ContactInfo();
            contact.ipAddress = ip;
            contact.name = username;

            String jsonData = JsonConvert.SerializeObject(contact);

            Console.WriteLine("JSON data: " + jsonData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.0.105:5050/users/contacts/");
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
    }
}

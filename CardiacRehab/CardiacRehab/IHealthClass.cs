using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CardiacRehab
{
    /// <summary>
    /// Majority of the code was taken from https://github.com/iHealthLabs/Sandbox-V2 where
    /// the official iHealth sandbox API is supplied from.  Some modifications were made to be
    /// integrated into the system.
    /// </summary>
    public class IHealthAuthData
    {
        public String APIName;
        public String AccessToken;
        public int Expires;
        public String RefreshToken;
        public String UserID;
        public String client_para;
    }

    /// <summary>
    /// Change Unix to UTC
    /// </summary>
    public static class UnixTime
    {

        /// <summary>
        /// UTC 1970-1-1 00:00:00
        /// </summary>
        private static readonly DateTime UTCUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a DateTime to unix time. Unix time is the number of seconds 
        /// between 1970-1-1 0:0:0.0 (unix epoch) and the time (UTC).
        /// </summary>
        /// <param name="time">utc</param>
        /// <returns>The number of seconds between Unix epoch and the input time</returns>
        public static long ToUnixTime(DateTime time)
        {
            return (long)(time - UTCUnixEpoch).TotalSeconds;
        }


        /// <summary>
        /// Converts a long representation of a unix time into a DateTime. Unix time is 
        /// the number of seconds between 1970-1-1 0:0:0.0 (unix epoch) and the time (UTC).
        /// </summary>
        /// <param name="unixTime">The number of seconds since Unix epoch (must be >= 0)</param>
        /// <returns>A UTC DateTime object representing the unix time</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "unix", Justification = "UNIX is a domain term")]
        public static DateTime FromUnixTime(long unixTime)
        {
            if (unixTime < 0)
                throw new ArgumentOutOfRangeException("unixTime");

            return UTCUnixEpoch.AddSeconds(unixTime);
        }
    }

    class IHealthClass
    {
        #region You must modify this part
        private string client_id = "2b5201ab0aec40edae212c704abe3eed";
        private string client_secret = "358d6ccf4309447fa433dc1d58eb327a";
        private string redirect_uri = "http://spaces.facsci.ualberta.ca/ammi/projects/medical-projects/remote-rehab-bike-projects/";
        private string sc = "D1F213782A124610A441F28C50BA97FB";
        private string sv_OpenApiBP = "4025759B32F24B83AA07577D8CADBBB5";

        #endregion

        private string response_type_code = "code";
        private string response_type_refresh_token = "refresh_token";
        private string grant_type_authorization_code = "authorization_code";
        private string APIName_BP = "OpenApiBP";
        private string url_authorization = "https://api.ihealthlabs.com:8443/OpenApiV2/OAuthv2/userauthorization/";
        private string url_bp_data = "https://api.ihealthlabs.com:8443/openapiv2/user/{0}/bp.json/";
        private string url_bp_data_xml = "https://api.ihealthlabs.com:8443/openapiv2/user/{0}/bp.xml/";

        private string url_bp_data_client = "https://api.ihealthlabs.com:8443/openapiv2/application/bp.json/";

        private String access_code = null;
        private IHealthAuthData access_token = null;
        public IHealthAuthData Access_token
        {
            get
            {
                return access_token;
            }
            set 
            {
                access_token = Access_token;
            }
        }
        bool wasAuthenticated = false;
        private mshtml.HTMLDocument htmldoc;
        private int patientIndex;
        private String ihealth_email = null;
        private String ihealth_password = null;
        private PatientWindow currentWindow;

        public IHealthClass(int index, PatientWindow window)
        {
            patientIndex = index;
            currentWindow = window;
            GetIHealthAccount();
        }

        private void GetIHealthAccount()
        {
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("username, password", "authentication", "role='iHealth" + patientIndex.ToString().Trim() + "'");
            if(result[0] != null)
            {
                ihealth_email = result[0][0].Trim();
                ihealth_password = result[1][0].Trim();
            }
            else
            {
                MessageBox.Show("No iHealth Account detected.");
            }
        }

        public void GetCode()
        {
            if(ihealth_email != null)
            {
                string url = url_authorization
               + "?client_id=" + client_id
               + "&response_type=" + response_type_code
               + "&redirect_uri=" + HttpUtility.UrlEncode(redirect_uri)
               + "&APIName=" + APIName_BP;

                AuthenticateToCloud(url);
            }
            else
            {
                MessageBox.Show("No iHealth Account was detected.");
            }
            
        }

        private void AuthenticateToCloud(String url)
        {
            WebBrowser br = new WebBrowser();
            br.Visibility = Visibility.Hidden;
            currentWindow.grid1.Children.Add(br);
            br.LoadCompleted += br_LoadCompleted;
            br.Navigate(url);
        }

        private void br_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebBrowser br = sender as WebBrowser;
            if (!wasAuthenticated)
            {
                // replace below with iHealth account information

                htmldoc = br.Document as mshtml.HTMLDocument;
                htmldoc.getElementById("txtUserName").innerText = ihealth_email;
                htmldoc.getElementById("txtPsw").innerText = ihealth_password;
                htmldoc.getElementById("Button1").click();

                wasAuthenticated = true;
            }
            else
            {
                Uri redirectedUri = new Uri(br.Source.ToString());
                access_code = HttpUtility.ParseQueryString(redirectedUri.Query).Get("code");

                if (access_code != null)
                {
                    GetAccessToken();
                }
                else
                {
                    // Either there was an error retrieving the access_code or
                    // this user has not given permission for the application to get the data
                    // so link the patient to the application.
                    htmldoc = br.Document as mshtml.HTMLDocument;
                    mshtml.IHTMLElement linkButton = htmldoc.getElementById("btnLink");
                    if(linkButton != null)
                    {
                        linkButton.click();
                    }
                }
            }

        }

        private void GetAccessToken()
        {
            string url = url_authorization
           + "?client_id=" + client_id
           + "&client_secret=" + client_secret
           + "&code=" + access_code
           + "&grant_type=" + grant_type_authorization_code
           + "&redirect_uri=" + HttpUtility.UrlEncode(redirect_uri);

            byte[] getData = new WebClient().DownloadData(url);
            String receivedData = Encoding.UTF8.GetString(getData);

            if (receivedData.StartsWith("{\"Error\":"))
            {
                MessageBox.Show("Error at retrieving access token");
            }
            else
            {
                access_token = JsonConvert.DeserializeObject<IHealthAuthData>(receivedData);
            }
        }

        public String GetBloodPressure(String startTime, String endTime)
        {
            string url = string.Format(url_bp_data, access_token.UserID)
                    + "?access_token=" + access_token.AccessToken
                    + "&client_id=" + client_id
                    + "&client_secret=" + client_secret
                    + "&redirect_uri=" + HttpUtility.UrlEncode(redirect_uri)
                    + "&sc=" + sc
                    + "&sv=" + sv_OpenApiBP
                    + "&locale=mmHg";

            if (startTime != "")
                url += "&start_time=" + startTime;

            if (endTime != "")
                url += "&end_time=" + endTime;

            byte[] getData = new WebClient().DownloadData(url);
            String receivedData = Encoding.UTF8.GetString(getData);

            if (receivedData.StartsWith("{\"Error\":"))
            {
                Console.WriteLine(receivedData);
                return "Error in retreiving BP";
            }
            else
            {
                return ProcessBPData(receivedData);
            }
        }
        
        /// <summary>
        /// Extract the systolic and diastolic data from the iHealth Cloud data.
        /// </summary>
        /// <param name="received">blood pressure data pulled from iHealth Cloud</param>
        /// <returns>Processed string containing data in format systolic1,diastolic1/systolic,diastolic2...etc</returns>
        private String ProcessBPData(String received)
        {
            String[] modBPData = received.Split(',');
            String data = "";
            bool gotdata = false;

            // extract systolic and diastolic and return it as systolic, diastolic/systolic,diastolic...etc
            for(int i=0; i < modBPData.Length; i++)
            {
                if(modBPData[i].Contains("HP"))
                {
                    String[] temp = modBPData[i].Trim().Split(':');
                    data += temp[1] + ",";
                }
                else if(modBPData[i].Contains("LP"))
                {
                    String[] temp = modBPData[i].Trim().Split(':');
                    data += temp[1];
                    gotdata = true;
                }
                else
                {
                    gotdata = false;
                }
                if(gotdata)
                {
                    data += "/";
                }
            }
            return data;
        }
        
        // later need a refresh method to keep the access token valid. (need to get expiry time on access token)

    }
}

namespace Pluton
{
    using System;
    using System.Net;
    using System.Text;

    public class Web
    {
        public string GET(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public string POST(string url, string data)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                byte[] bytes = client.UploadData(url, "POST", Encoding.ASCII.GetBytes(data));
                return Encoding.ASCII.GetString(bytes);
            }
        }

        public string POSTJSON(string url, string json)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                byte[] bytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(json));
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
namespace Jean_Doe.Common
{
    public class PCS_client
    {
        public string GetAccessTokenPage()
        {
            var url = "https://openapi.baidu.com/oauth/2.0/authorize";
            var dic = new NameValueCollection
            {
                {"client_id","DtNgikRWSM7qHUShIOAmvw5y"},
                {"response_type","token"},
                {"redirect_uri","oob"},
                {"scope","netdisk"},
                {"force_login","1"},
            };
            return url + ToQueryString(dic);
        }
        public string GetDownloadUrl(string path)
        {
            access_token = Global.AppSettings["baidu_access_token"];
            if (string.IsNullOrEmpty(access_token))
            {
                return "error: access token not found";
            }
            var url=  "https://d.pcs.baidu.com/rest/2.0/pcs/file";
            var dic = new NameValueCollection
            {
                {"method","download"},
                {"access_token",access_token},
                {"path",path},
            };
            return url + ToQueryString(dic);
        }
        string access_token;
        public string DownloadFile(string path, string localFileName)
        {
            access_token = Global.AppSettings["baidu_access_token"];
            if (string.IsNullOrEmpty(access_token)) {
                return "error: access token not found";
            }
            var url = GetDownloadUrl(path); 
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";
                client.DownloadFile(url, localFileName);
            }
            return "ok";
        }
        string Get(string uri, NameValueCollection dic)
        {
            string res = null;
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";
                res = client.DownloadString(uri + ToQueryString(dic));
            }
            return res;
        }
        string Post(string uri, NameValueCollection pairs)
        {
            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";
                response = client.UploadValues(uri, pairs);
            }
            return Encoding.UTF8.GetString(response);
        }
        string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value, Encoding.Default)))
                .ToArray();
            var res = "?" + string.Join("&", array);
            return res;
        }

    }
}

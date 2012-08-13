using System;
using System.Net;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Threading.Tasks;
namespace Jean_Doe.Common
{
    public static class DownloadHelper
    {
        public static string GetUrlMp3(this XmlDocument xdoc)
        {
            string res = null;
            try
            {
                var loc = xdoc.GetElementsByTagName("location");
                res = loc[0].InnerText.DicipherCaesar();
            }
            catch { }
            return res;
        }
        public static string GetUrlLyrics(this XmlDocument xdoc)
        {
            string res = null;
            try
            {
                res = xdoc.GetElementsByTagName("lyric")[0].InnerText.Trim();
            }
            catch { }
            return res;
        }
        public static string GetUrlCover(this XmlDocument xdoc)
        {
            string res = null;
            try
            {
                string imgUrl = xdoc.GetElementsByTagName("pic")[0].InnerText;
                string imgSuffix = imgUrl.Substring(imgUrl.LastIndexOf('.'));
                string baseUrl = imgUrl.Replace("_1" + imgSuffix, "");
                string[] suffix = new[]
				    {
					    imgSuffix, 
					    "_2" + imgSuffix, 
					    "_1" + imgSuffix
				    };

                for (int i = 0; i < suffix.Length; i++)
                {
                    string imgURL = baseUrl + suffix[i];
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(imgURL);
                    HttpWebResponse r = (HttpWebResponse)req.GetResponse();
                    if (r.ContentType.ToLower().StartsWith("image/"))
                    {
                        res = imgURL;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return res;
        }
    }


}

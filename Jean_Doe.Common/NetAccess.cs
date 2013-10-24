using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using Artwork.MessageBus;
using System.Windows;
using System.Web.Script.Serialization;
using System.Collections;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
namespace Jean_Doe.Common
{
    public static class NetAccess
    {
        static object lck = new object();
        public static void CancelAsync()
        {
            cancelToken.Cancel();
            cancelToken = new CancellationTokenSource();
        }
        static List<CancellationTokenSource> cancelTokens = new List<CancellationTokenSource>();
        static CancellationTokenSource cancelToken = new CancellationTokenSource();
        public async static Task<dynamic> Json(string url, params string[] ps)
        {

            var len = ps.Length;
            if (len % 2 != 0)
                throw new ArgumentException();
            var dic = new Dictionary<string, string>();
            if (len > 0)
            {
                for (int i = 0; i < len / 2; i++)
                {
                    dic[ps[2 * i]] = ps[2 * i + 1];
                }
                url = url.WithParams(dic);
            }
            Guid o = Guid.NewGuid();
            MessageBus.Instance.Publish(new MsgSetBusy(o, true));
            var s = await DownloadStringAsync(url);
            MessageBus.Instance.Publish(new MsgSetBusy(o, false));
            return s.ToDynamicObject();
        }
        public async static Task<string> DownloadStringAsync(string url)
        {
            var client = new HttpClient();
            string res = null;
            try
            {
                res = await client.GetStringAsync(url);
            }
            catch (Exception e)
            {

            }
            finally
            {
                client.Dispose();
            }
            return res;
        }

        public async static Task<string> GetUrlLrc(string songId)
        {
            var json = await NetAccess.Json(XiamiUrl.url_song, "id", songId);
            if (json == null) return null;
            try
            {
                return json.lyric as string;
            }
            catch
            {
                return null;
            }
        }
        static Dictionary<string, int> TrackNoCache = new Dictionary<string, int>();
        static object l = new object();
        public static dynamic ToDynamicObject(this string json)
        {
            dynamic res = null;
            try
            {
                res = JObject.Parse(json);
            }
            catch
            {
                try
                {
                    res = JArray.Parse(json);
                }
                catch (Exception)
                {
                }
            }
            return res;

        }
    }
}

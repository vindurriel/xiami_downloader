using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Web;
namespace Jean_Doe.Common
{
    public class XiamiClient
    {
        static string ApiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xiami_api.exe");
        static XiamiClient _inst = null;
        public static XiamiClient GetDefault()
        {
            if (_inst == null)
            {
                _inst = new XiamiClient();
                _inst.isLoggedIn = File.Exists(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "access_token"));
            }
            return _inst;
        }
        public async Task Fav_Song(string songId)
        {
            await Call_xiami_api("Library.addSong", "id=" + songId, "grade=5");
        }
        public async Task Unfav_Song(string songId)
        {
            await Call_xiami_api("Library.removeSong", "id=" + songId);
        }

        string get_api_signature(NameValueCollection dic, string secret)
        {
            var res = "";
            var keys = dic.AllKeys.OrderBy(x => x);
            foreach (var k in keys)
            {
                res += k + dic[k];
            }
            res += secret;
            res = Encoding.Default.GetBytes(res).ToMD5();
            return res;
        }


        public async Task<dynamic> GetGuess()
        {
            dynamic json = await Call_xiami_api("Recommend.getSongList"); //DailySongs
            var res = new DynamicJsonObject(new Dictionary<string, object> { { "songs", json } });
            return res;
        }
        public async Task<dynamic> GetDailyRecommend()
        {
            return await Call_xiami_api("Recommend.DailySongs"); //DailySongs
        }
        public async Task<dynamic> GetUserMusic(string t, int page)
        {
            if (t == "collect")
                return await Call_xiami_api("Collects.getLibCollects", string.Format("page={0}", page));
            var s = t[0].ToString().ToUpper() + t.Substring(1);
            return await Call_xiami_api(string.Format("Library.get{0}s", s), string.Format("page={0}", page));
        }
        #region obsolete
        public async Task _Login()
        {
            Global.AppSettings["xiami_avatar"] = "";
            Global.AppSettings["xiami_nick_name"] = "";
            var res = await RunProgramHelper.RunProgramGetOutput(ApiPath, new[]{
                "get_new_token",
                Global.AppSettings["xiami_username"],
                Global.AppSettings["xiami_password"],
            });
            var json = res.ToDynamicObject();
            if (json.error != null)
            {
                MessageBox.Show(json.error.ToString());
                return;
            }
            var r = await Call_xiami_api("Members.showUser");
            if (r.error != null)
            {
                MessageBox.Show(r.error.ToString());

                return;
            }

            Global.AppSettings["xiami_uid"] = r.user_id.ToString();
            Global.AppSettings["xiami_nick_name"] = string.Format("来自{0}的{1}", r.city, r.nick_name);
            string avatarUrl = r.avatar.ToString();
            isLoggedIn = true;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                Global.AppSettings["xiami_avatar"] = avatarUrl;
            }
        }
        public async Task<dynamic> _Call_xiami_api(string methodName, params string[] args)
        {
            var p = new List<string>() { "api_get", methodName };
            if (args != null)
                p.AddRange(args);
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            string res = await RunProgramHelper.RunProgramGetOutput(ApiPath, p.ToArray());
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, false));
            var json = res.ToDynamicObject();
            if (!(json is string) && !(json is Array) && json.error != null)
            {
                MessageBox.Show(json.error.ToString());
                return null;
            }

            return json;
        }
        #endregion
        static string url_api = "http://api.xiami.com/api";
        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }
        public async Task<dynamic> Call_xiami_api(string methodName, params string[] args)
        {
            return await Call_xiami_api(methodName, false, args);
        }
        public async Task<dynamic> Call_xiami_api(string methodName, bool useGet, params string[] args)
        {
            var dic = new NameValueCollection{
                       {"method",methodName},
                        {"api_key",client_id},
                        {"call_id",DateTimeToUnixTimestamp(DateTime.Now).ToString() },
                        {"av","XMusic_1.1.1.4009"},
            };
            foreach (var item in args)
            {
                var s = item.Split('=');
                string k = s[0], v = s[1];
                dic[s[0]] = s[1];
            }
            dic["api_sig"] = get_api_signature(dic, client_secret);
            var tokenFile = Global.CWD("access_token");
            if (!File.Exists(tokenFile))
                await Login();
            dic["access_token"] = File.ReadAllText(tokenFile);
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            string res;
            if (useGet)
                res = await Http.Get(url_api, dic);
            else
                res = await Http.Post(url_api, dic);
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, false));
            var json = res.ToDynamicObject();
            if (!(json is string) && !(json is Array) && json.err != null)
            {
                //MessageBox.Show(json.err.ToString());
                return null;
            }
            return json.data;
        }

        static string client_id = "55ee94348aeba2635326059e60d20a49";
        static string client_secret = "ec253979c7f51e10010a30241c2ca2de";
        static string url_new_token = "http://api.xiami.com/api/oauth2/token";
        public async Task Login()
        {
            var password = Global.AppSettings["xiami_password"];
            var username = Global.AppSettings["xiami_username"];
            if (password.Length != 32)
                password = password.ToMD5();
            var resp = await Http.Post(url_new_token, new NameValueCollection{
                {"grant_type","password"},
                {"username",username},
                {"password",password},
                {"client_id",client_id},
                {"client_secret",client_secret}
            });
            var json = resp.ToDynamicObject();
            if (json.error != null)
            {
                MessageBox.Show(json.error.ToString());
                return;
            }
            File.WriteAllText(Global.CWD("access_token"), json["access_token"]);
            var r = await Call_xiami_api("Members.showUser");
            if (r.error != null)
            {
                MessageBox.Show(r.error.ToString());

                return;
            }

            Global.AppSettings["xiami_uid"] = r.user_id.ToString();
            Global.AppSettings["xiami_nick_name"] = string.Format("来自{0}的{1}", r.city, r.nick_name);
            string avatarUrl = r.avatar.ToString();
            isLoggedIn = true;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                Global.AppSettings["xiami_avatar"] = avatarUrl;
            }

        }
        bool isLoggedIn;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
        }
    }
    public static class Http
    {
        public static async Task<string> Post(string uri, NameValueCollection pairs)
        {
            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                response = await client.UploadValuesTaskAsync(uri, pairs);
            }
            return Encoding.UTF8.GetString(response);
        }
        public static async Task<string> Get(string uri, NameValueCollection dic)
        {
            string res = null;
            using (WebClient client = new WebClient())
            {
                res = await client.DownloadStringTaskAsync(uri + ToQueryString(dic));
            }
            return res;
        }
        static string ToQueryString(NameValueCollection nvc)
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

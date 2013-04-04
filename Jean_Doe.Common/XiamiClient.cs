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
namespace Jean_Doe.Common
{
    class MyHttpClienHanlder : HttpClientHandler
    {
        public MyHttpClienHanlder(CookieContainer cj)
        {
            UseCookies = true;
            CookieContainer = cj;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Referrer = new Uri("http://www.xiami.com");
            request.Headers.Add("UserAgent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727)");
            return base.SendAsync(request, cancellationToken);
        }
    }
    public class XiamiClient
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            Int32 dwFlags,
            IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
        private string cookiesPath = "cookies.dat";

        public string CookiesPath
        {
            get { return cookiesPath; }
            set { cookiesPath = value; }
        }

        static XiamiClient _inst = null;
        public static XiamiClient GetDefault()
        {
            if (_inst == null)
                _inst = new XiamiClient();
            return _inst;
        }
        public XiamiClient()
        {
            LoadCookies();
        }
        CookieContainer cookieJar = new CookieContainer();
        public string Username { get; set; }
        public string Password { get; set; }
        public void SaveCookies()
        {
            PersistHelper.SaveBin(cookieJar, CookiesPath);
        }
        public void LoadCookies()
        {
            var c = PersistHelper.LoadBin<CookieContainer>(CookiesPath);
            if (c == null) return;
            cookieJar = c;
        }
        HttpClient client = null;
        void ensureClient()
        {
            if (client == null)
            {
                client = new HttpClient(new MyHttpClienHanlder(cookieJar));
                client.BaseAddress = new Uri("http://www.xiami.com");
            }
        }
        public async Task<string> Fav_Song(string songId)
        {
            var xm_favUrl = "http://www.xiami.com/ajax/addtag";
            var favData = new Dictionary<string, string>{
                {"type", "3"}, 
                {"share", "1"}, 
                {"shareAll", "all"},
                {"id",songId},
            };
            var postObj = new FormUrlEncodedContent(favData);
            ensureClient();
            var resp = await client.PostAsync(new Uri(xm_favUrl), postObj);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (content.Contains("ok"))
                return "ok";
            return content;
        }
        public async Task<string> Unfav_Song(string songId)
        {
            var xm_favUrl = "http://www.xiami.com/ajax/space-lib-del?song_id=" + songId;
            ensureClient();
            var resp = await client.GetAsync(new Uri(xm_favUrl));
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (content.Contains("ok"))
                return "ok";
            return content;
        }
        public async Task<string> AddSongToCollect(string songId, string collectId)
        {
            var xm_favUrl = "http://www.xiami.com/song/collect";
            var favData = new Dictionary<string, string>{
                {"tag_name", ""}, 
                {"description", ""}, 
                {"submit", "保 存"}, 
                {"list_id", collectId},
                {"id",songId},
            };
            var postObj = new FormUrlEncodedContent(favData);
            ensureClient();
            var resp = await client.PostAsync(new Uri(xm_favUrl), postObj);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();

            return "ok";
        }
        public async Task<string> GetString(string url)
        {
            ensureClient();
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                return "";
            }
        }
        static string ApiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xiami_api.exe");
        async Task<dynamic> call_xiami_api(string methodName, params string[] args)
        {
            var p = new List<string>() { "api_get", methodName };
            if (args != null)
                p.AddRange(args);
            string res = await RunProgramHelper.RunProgramGetOutput(ApiPath, p.ToArray());
            var json = res.ToDynamicObject();
            if (json.error != null)
            {
                MessageBox.Show(json.error.ToString());
                return null;
            }
            return json;
        }
        public async Task<dynamic> GetDailyRecommend()
        {
            return await call_xiami_api("Recommend.DailySongs");
        }
        public async Task<dynamic> GetUserMusic(string t, int page)
        {
            if (t == "collect")
                return await call_xiami_api("Collects.getLibCollects", string.Format("page={0}", page));
            var s = t[0].ToString().ToUpper() + t.Substring(1);
            return await call_xiami_api(string.Format("Library.get{0}s", s), string.Format("page={0}", page));
        }
        public async Task Login()
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
            var r = await call_xiami_api("Members.showUser");
            if (r.error != null)
            {
                MessageBox.Show(r.error.ToString());
                return;
            }
            Global.AppSettings["xiami_uid"] = r.user_id.ToString();
            Global.AppSettings["xiami_nick_name"] = string.Format("来自{0}的{1}",r.city,r.nick_name);
            string avatarUrl = r.avatar.ToString();
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                var bytes = await new HttpClient().GetByteArrayAsync(avatarUrl);
                var imgFile = Path.Combine(Global.BasePath, "cache", Global.AppSettings["xiami_uid"] + ".user");
                File.WriteAllBytes(imgFile, bytes);
                Global.AppSettings["xiami_avatar"] = imgFile;
            }
        }
    }
}

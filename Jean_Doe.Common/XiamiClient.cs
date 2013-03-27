using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
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
            request.Headers.Referrer = new Uri("http://www.xiami.com/web/login");
            request.Headers.Add("UserAgent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727)");
            return base.SendAsync(request, cancellationToken);
        }
    }
    public class XiamiClient
    {
        static XiamiClient _inst = null;
        public static XiamiClient GetDefault()
        {
            if (_inst == null)
                _inst = new XiamiClient();
            return _inst;
        }
        public XiamiClient()
        {
            LoadCookies("cookies.dat");
        }
        CookieContainer cookieJar = new CookieContainer();
        public string Username { get; set; }
        public string Password { get; set; }
        string cookiesPath = null;
        public void SaveCookies(string path)
        {
            PersistHelper.SaveBin(cookieJar, path);
        }
        public void LoadCookies(string path)
        {
            var c = PersistHelper.LoadBin<CookieContainer>(path);
            if (c == null) return;
            cookiesPath = path;
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
            if (client == null)
            {
                client = new HttpClient(new MyHttpClienHanlder(cookieJar));
                client.BaseAddress = new Uri("http://www.xiami.com");
            }
            var resp = await client.PostAsync(new Uri(xm_favUrl), postObj);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();

            return "ok";
        }
        public async Task<string> GetString(string url)
        {
            ensureClient();
            return await client.GetStringAsync(url);
        }
        public async Task<string> Login(string validationCode = null)
        {
            try
            {
                string url = "http://www.xiami.com/web/login";
                Uri address = new Uri(url);
                if (client == null)
                {
                    client = new HttpClient(new MyHttpClienHanlder(cookieJar));
                    client.BaseAddress = new Uri("http://www.xiami.com");
                }
                if (string.IsNullOrWhiteSpace(validationCode))
                {
                    var resp = await client.GetAsync(url);
                    if (resp.StatusCode == HttpStatusCode.Redirect)
                    {
                        return "ok";
                    }
                    var content = await resp.Content.ReadAsStringAsync();
                    var m = Regex.Match(content, "src=\"(/coop/checkcode\\?forlogin=1&\\d+)\"");
                    if (m.Success)
                    {
                        var codeImage = m.Groups[1].Value;
                        resp = await client.GetAsync(codeImage);
                        resp.EnsureSuccessStatusCode();
                        var bytes = await resp.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(Path.Combine(Global.BasePath, "capcha.bmp"), bytes);
                        return "validation required:";
                    }
                }
                var postData = new Dictionary<string, string>{
                    {"email", Username},
                    {      "password", Password},
                    {     "remember", "1"},
                    {     "LoginButton",  Uri.EscapeUriString("登录")},
                    //{     "done", "/"},
                    //{     "type", ""},
                    };
                if (!string.IsNullOrWhiteSpace(validationCode))
                {
                    postData.Add("validate", validationCode);
                }
                var postObj = new FormUrlEncodedContent(postData);
                var r = await client.PostAsync(url, postObj);
                r.EnsureSuccessStatusCode();
                var c = await r.Content.ReadAsStringAsync();
                if (c.Contains("会员登录"))
                {
                    if (c.Contains("验证码错误"))
                        return "validation code incorrect";
                    else if (c.Contains("密码错误"))
                        return "Email or password incorrect";
                }
                SaveCookies(cookiesPath);
                return "ok";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}

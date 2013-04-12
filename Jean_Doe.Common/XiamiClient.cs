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
    public class XiamiClient
    {
        static string ApiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xiami_api.exe");
        static XiamiClient _inst = null;
        public static XiamiClient GetDefault()
        {
            if (_inst == null)
                _inst = new XiamiClient();
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
        public async Task<dynamic> Call_xiami_api(string methodName, params string[] args)
        { 
            var p = new List<string>() { "api_get", methodName };
            if (args != null)
                p.AddRange(args);
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            string res = await RunProgramHelper.RunProgramGetOutput(ApiPath, p.ToArray());
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this,false));
            var json = res.ToDynamicObject();
            if (!(json is string) && !(json is Array) && json.error != null)
            {
                MessageBox.Show(json.error.ToString());
                return null;
            }

            return json;
        }
        public async Task<dynamic> GetGuess()
        {
            dynamic json = await Call_xiami_api("Recommend.getSongList"); //DailySongs
            var res=new DynamicJsonObject(new Dictionary<string,object>{{"songs",json}});
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
            var r = await Call_xiami_api("Members.showUser");
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

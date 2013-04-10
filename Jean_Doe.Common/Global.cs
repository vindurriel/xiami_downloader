using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Jean_Doe.Common
{

    public static class Global
    {
        static readonly ObservDict<string, string> DefaultSettings = new ObservDict<string, string>{
            {"DownloadFolder", "D:\\music"},
            {"ConfigPath", "config.ini"},
            {"FolderPattern", ""},
            {"SongnamePattern", "%ArtistName - %Name - %AlbumName"},
            {"ActivePage","1"},
            {"WindowPos","0,0,0,0"},
            {"EnableMagnet","1"},
            {"MaxConnection","10"},
            {"ColorSkin","#ffaaff"},
            {"SearchResultType","song"},
            {"PlayNextMode","Sequential"},
            {"xiami_uid","86"},
            {"xiami_avatar",""},
            {"xiami_username",""},
            {"xiami_password",""},
            {"xiami_nick_name",""},
            {"ShowNowPlaying","1"},
            {"ShowLyric","1"},
        };
        public static Dictionary<string, Dictionary<string,string>> ValueOptions = new Dictionary<string, Dictionary<string,string>>
        {
            {"MaxConnection", new Dictionary<string,string>{
                {"10","10"},
                {"20","20"},
                {"不限制","9999"},
            }},
            {"SongnamePattern", new Dictionary<string,string>{
                {"艺术家 - 歌曲名 - 专辑名","%ArtistName - %Name - %AlbumName"},
                {"歌曲名 - 歌曲ID","%Name - %Id"},
                {"艺术家 - 歌曲名","%ArtistName - %Name"},
            }},
            {"FolderPattern", new Dictionary<string,string>{
                {"不建立目录",""},
                {"艺术家\\专辑","%ArtistName\\%AlbumName"},
                {"仅专辑","%AlbumName"},
                {"仅艺术家","%ArtistName"},
            }},            
            {"PlayNextMode", new Dictionary<string,string>{
                {"顺序","Sequential"},
                {"随机","Random"},
                {"单曲循环","Repeat"},
            }},
        };
        public class ObservDict<Tkey, TValue> : Dictionary<Tkey, TValue>
        {
            public new TValue this[Tkey key]
            {
                get
                {
                    if (!ContainsKey(key)) return default(TValue);
                    return base[key];
                }
                set
                {
                    base[key] = value;
                    Global.RaiseEvent(key as string);
                }
            }
        }
        public static ObservDict<string, string> AppSettings = new ObservDict<string, string>();
        public static string BasePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString(), "XiamiDownloader"); } }

        public static void LoadSettings()
        {
            AppSettings = DefaultSettings;
            var config = Path.Combine(BasePath, AppSettings["ConfigPath"]);
            if (!File.Exists(config))
                return;
            try
            {
                var lines = File.ReadAllLines(config);
                foreach (var line in lines)
                {
                    var x = line.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (x.Length != 2) continue;
                    var key = x[0].Trim();
                    var value = x[1].Trim();
                    if (!AppSettings.ContainsKey(key))
                        continue;
                    AppSettings[key] = value;
                }
            }
            catch
            {
            }
        }
        public static void SaveSettings()
        {
            try
            {
                var lines = AppSettings.Select(item => item.Key.Trim() + " = " + item.Value.Trim()).ToList();
                var config = Path.Combine(BasePath, AppSettings["ConfigPath"]);
                File.WriteAllLines(config, lines);
            }
            catch
            {
            }
        }
        public static void ListenToEvent(string @event, Action<string> a)
        {
            if (string.IsNullOrEmpty(@event)) return;
            if (!dict.ContainsKey(@event))
                dict[@event] = new List<Action<string>>();
            dict[@event].Add(a);
            if (AppSettings.ContainsKey(@event))
                a(AppSettings[@event]);
        }
        static void RaiseEvent(string @event)
        {
            if (!dict.ContainsKey(@event)) return;
            if (!AppSettings.ContainsKey(@event)) return;
            var value = AppSettings[@event];
            foreach (var action in dict[@event])
            {
                try
                {
                    action(value);
                }
                catch { }
            }
        }
        public static Dictionary<string, List<Action<string>>> dict = new Dictionary<string, List<Action<string>>>();
    }
}

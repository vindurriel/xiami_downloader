using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Jean_Doe.Common
{

    public static class Global
    {
        public class ObservDict<Tkey, TValue> : Dictionary<Tkey, TValue>
        {
            public new TValue this[Tkey key]
            {
                get
                {
                    if(!ContainsKey(key)) return default(TValue);
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
        static readonly ObservDict<string, string> DefaultSettings = new ObservDict<string, string>{
            {"DownloadFolder", "D:\\music"},
            {"SearchByKeyPath", "history_key.xml"},
            {"SearchByUrlPath", "history_url.xml"},
            {"SearchByIdPath", "history_id.xml"},
            {"ConfigPath", "config.ini"},
            {"FolderPattern", ""},
            {"SongnamePattern", "%ArtistName - %Name - %AlbumName"},
            {"ActivePage","1"},
            {"WindowPos","0,0,0,0"},
            {"EnableMagnet","1"},
            {"MaxConnection","10"},
            {"ColorSkin","#ffaaff"},
        };
        public static void LoadSettings()
        {
            AppSettings = DefaultSettings;
            var config = Path.Combine(BasePath, AppSettings["ConfigPath"]);
            if(!File.Exists(config))
                return;
            try
            {
                var lines = File.ReadAllLines(config);
                foreach(var line in lines)
                {
                    var x = line.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if(x.Length != 2) continue;
                    var key = x[0].Trim();
                    var value = x[1].Trim();
                    if(!AppSettings.ContainsKey(key))
                        AppSettings.Add(key, value);
                    else
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
            if(string.IsNullOrEmpty(@event)) return;
            if(!dict.ContainsKey(@event))
                dict[@event] = new List<Action<string>>();
            dict[@event].Add(a);
        }
        static void RaiseEvent(string @event)
        {
            if(!dict.ContainsKey(@event)) return;
            if(!AppSettings.ContainsKey(@event)) return;
            var value = AppSettings[@event];
            foreach(var action in dict[@event])
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

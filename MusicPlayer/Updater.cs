using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jean_Doe.Common;
using System.IO;
using Jean_Doe.Downloader;
using System.IO.Compression;
using System.Text.RegularExpressions;
namespace MusicPlayer
{
    class Updater
    {
        static string cwd(params string[] strs)
        {
            var s = Path.Combine(strs);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s);
        }
        public static bool IsLatest()
        {
            var client = new System.Net.WebClient();
            client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            var res = client.DownloadString("https://api.github.com/repos/vindurriel/xiami_downloader/commits");
            string latest = res.ToDynamicObject()[0]["sha"];
            File.WriteAllText(cwd("latest.txt"), latest);
            var current = "";
            if (File.Exists(cwd("current.txt")))
                current = File.ReadAllText(cwd("current.txt"));
            return current == latest;
        }
        public static void Download()
        {
            var content = new
            {
                scopes = new[] { "repo" },
                note = "API test",
            };
            var client = new System.Net.WebClient();
            var dir_latest = cwd("latest");
            client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            client.DownloadFile("https://github.com/vindurriel/xiami_downloader/archive/master.zip", cwd("latest.zip"));
            if (Directory.Exists(dir_latest))
                Directory.Delete(dir_latest, true);
            Directory.CreateDirectory(dir_latest);
            var z = ZipFile.OpenRead(cwd("latest.zip"));
            var re = new Regex(@"Jean_Doe.Output/.+");
            foreach (var f in z.Entries.Where(x => re.IsMatch(x.FullName) && x.Name!="XiamiUpdater.exe"))
            {
                f.ExtractToFile(cwd("latest", f.Name), true);
            }
            File.CreateText(cwd("needs_update"));
        }
    }
}

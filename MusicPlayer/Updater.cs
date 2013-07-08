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
using System.ComponentModel;
using Artwork.MessageBus;
namespace MusicPlayer
{
    class Updater
    {
        public static bool IsLatest()
        {
            var client = new System.Net.WebClient();
            client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            var res = client.DownloadString("https://api.github.com/repos/vindurriel/xiami_downloader/commits");
            string latest = res.ToDynamicObject()[0]["sha"];
            File.WriteAllText(Global.CWD("latest.txt"), latest);
            var current = "";
            if (File.Exists(Global.CWD("current.txt")))
                current = File.ReadAllText(Global.CWD("current.txt"));
            return current == latest;
        }
        public static void Download()
        {
            var client = new System.Net.WebClient();
            if(File.Exists(Global.CWD("update.zip")))
                File.Delete(Global.CWD("update.zip"));
            var d = new SoftwareDownloader();
            d.Download();
            //client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            //client.DownloadFile("https://github.com/vindurriel/xiami_downloader/archive/master.zip", Global.CWD("latest.zip"));
            
        }
    }
    public class MsgUpdateReady { }
    public class MsgUpdateProgressChanged
    {
        public string Percent { get; set; }
    }
    public class SoftwareDownloader : Downloader
    {
        public SoftwareDownloader()
        {
            Info = new DownloaderInfo
            {
                Url = "https://github.com/vindurriel/xiami_downloader/archive/master.zip",
                FileName = Global.CWD("latest.zip")
            };
        }
        protected override void OnProgressChanged()
        {
            Global.AppSettings["UpdateInfo"] = string.Format("{0}/{1}"
                    , curBytes.ToReadableString()
                    , totalBytes.ToReadableString()
                    );
        }
        public override void Process()
        {
            Global.AppSettings["UpdateInfo"] = "正在解压";
            var dir_latest = Global.CWD("latest");
            if (Directory.Exists(dir_latest))
                Directory.Delete(dir_latest, true);
            Directory.CreateDirectory(dir_latest);
            var z = ZipFile.OpenRead(Global.CWD("latest.zip"));
            var re = new Regex(@"Jean_Doe.Output/.+");
            foreach (var f in z.Entries.Where(x => re.IsMatch(x.FullName)))
            {
                f.ExtractToFile(Global.CWD("latest", f.Name), true);
            }
            File.Copy(Global.CWD("latest", "XiamiUpdater.exe"), Global.CWD("XiamiUpdater.exe"), true);
            File.WriteAllText(Global.CWD("needs_update"),"1");
            Global.AppSettings["UpdateInfo"] = "完成，需要重启";
            MessageBus.Instance.Publish(new MsgUpdateReady());
        }
    }
}

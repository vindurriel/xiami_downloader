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
        static string getRealUrl(string orig)
        {
            string res = null;
            using (var client = new System.Net.WebClient())
            {
                var r = client.DownloadString(orig);
                var re = new Regex("(http://d.pcs.+?)\"");
                var m = re.Match(r);
                if (m.Success)
                {
                    res = m.Groups[1].Value.Replace("&amp;", "&");
                }
            }
            return res;
        }
        public static bool IsLatest()
        {
            var client = new PCS_client();
            client.DownloadFile("/apps/folder1/version.txt", Global.CWD("latest.txt"));
            var latest = Global.CWD("latest.txt");
            if (!File.Exists(latest))
            {
                return true;
            }
            latest = File.ReadAllText(latest);
            var current = "";
            if (File.Exists(Global.CWD("current.txt")))
                current = File.ReadAllText(Global.CWD("current.txt"));
            return current == latest;
        }
        public static void Download()
        {
            var dest = Global.CWD("latest.zip");
            if (File.Exists(dest))
                File.Delete(dest);
            var url = new PCS_client().GetDownloadUrl("/apps/folder1/latest.zip");
            var d = new SoftwareDownloader
            {
                Info = new DownloaderInfo
                {
                    Url = url,
                    FileName = dest,
                }
            };
            d.Download();
        }
    }
    public class MsgUpdateReady { }
    public class MsgUpdateProgressChanged
    {
        public string Percent { get; set; }
    }
    public class SoftwareDownloader : Downloader
    {
        protected override void OnProgressChanged()
        {
            Global.AppSettings["UpdateInfo"] = string.Format("{0}/{1}"
                    , curBytes.ToReadableString()
                    , totalBytes.ToReadableString()
                    );
        }
        public override void Process()
        {
            try
            {
                Global.AppSettings["UpdateInfo"] = "正在解压";
                var dir_latest = Global.CWD("latest");
                if (Directory.Exists(dir_latest))
                    Directory.Delete(dir_latest, true);
                Directory.CreateDirectory(dir_latest);
                var z = ZipFile.OpenRead(Global.CWD("latest.zip"));
                var re = new Regex(@"Jean_Doe.Output/.+");
                foreach (var f in z.Entries)
                {
                    f.ExtractToFile(Global.CWD("latest", f.Name), true);
                }
                File.Copy(Global.CWD("latest", "XiamiUpdater.exe"), Global.CWD("XiamiUpdater.exe"), true);
                File.WriteAllText(Global.CWD("needs_update"), "1");
                Global.AppSettings["UpdateInfo"] = "完成，需要重启";
                MessageBus.Instance.Publish(new MsgUpdateReady());
            }
            catch (Exception e)
            {
                Jean_Doe.Common.Logger.Error(e);
            }

        }
    }
}

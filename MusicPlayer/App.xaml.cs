using Jean_Doe.Common;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Artwork.DataBus;
using Artwork.MessageBus;
using Jean_Doe.MusicControl;
namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var folder = Path.Combine(Global.BasePath, "cache");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            if (File.Exists("needs_update"))
            {
                RunProgramHelper.RunProgram("XiamiUpdater.exe", "");
                System.Environment.Exit(0);
            }

            var regedit=new Regedit();
            if(System.Environment.Is64BitOperatingSystem)
                regedit.SubKey = @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            else
                regedit.SubKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            var v = regedit.Read("xiami.exe");
            if(string.IsNullOrEmpty(v))
                regedit.Write("xiami.exe",10001);
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
                Global.ListenToEvent("baidu_access_token", (s) =>
                {
                    if (string.IsNullOrEmpty(s)) return;
                    Global.AppSettings["UpdateInfo"] = "正在检查更新";
                    if (!Updater.IsLatest())
                    {
                        Global.AppSettings["UpdateInfo"] = "正在下载更新";
                        Updater.Download();
                    }
                    else
                    {
                        Global.AppSettings["UpdateInfo"] = "已经是最新版本";
                    }
                });
                if (string.IsNullOrEmpty(Global.AppSettings["baidu_access_token"]))
                {
                    Global.AppSettings["UpdateInfo"] = "请先获取百度的令牌";
                    return;
                }
            });
            RunProgramHelper.RunProgram("xiami_player.exe", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
        }
       
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            var d = (sender as FrameworkElement).DataContext as CharmAction;
            if (d == null) return;
            d.Action(sender, e);
        }
    }
}

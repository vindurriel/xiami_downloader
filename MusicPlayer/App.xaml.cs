using Jean_Doe.Common;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Artwork.DataBus;
using Artwork.MessageBus;
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
            Task.Run(() =>
            {
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
            RunProgramHelper.RunProgram("xiami_player.exe", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
        }
        bool isPortTaken(int n)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(x => x.LocalEndPoint.Port == n);
        }
        int port = 500;
        public string getAvailablePort()
        {
            while (isPortTaken(port))
                port++;
            string res = port.ToString();
            port++;
            return res;
        }
    }
}

using Jean_Doe.Common;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Artwork.DataBus;
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
                if (!Updater.IsLatest())
                {
                    Updater.Download();
                }
            });
            string port1 = getAvailablePort();
            string port2 = getAvailablePort();
            DataBus.Set("port1", port1);
            DataBus.Set("port2", port2);
            RunProgramHelper.RunProgram("xiami_player.exe", System.Diagnostics.Process.GetCurrentProcess().Id.ToString(), port1, port2);
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

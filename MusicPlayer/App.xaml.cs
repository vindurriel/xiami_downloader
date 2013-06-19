using Jean_Doe.Common;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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
        }
    }
}

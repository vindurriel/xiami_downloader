using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Jean_Doe.Common;
namespace XiamiUpdater
{
    class Program
    {
        static string cwd(params string[] strs)
        {
            var s = Path.Combine(strs);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s);
        }
        static void Main(string[] args)
        {
            if (!File.Exists(cwd("needs_update"))) return;
            var dir_src = cwd("latest");
            if (!Directory.Exists(dir_src)) return;
            while (true)
            {
                var p = Process.GetProcessesByName("xiami");
                if (p.Length == 0)
                    break;
                p[0].Kill();
                Thread.Sleep(500);
            }
            while (true)
            {
                var p = Process.GetProcessesByName("xiami_player");
                if (p.Length == 0)
                    break;
                p[0].Kill();
                Thread.Sleep(500);
            }
            foreach (var f in Directory.EnumerateFiles(dir_src))
            {
                try
                {
                    var fname = Path.GetFileName(f);
                    if (fname == "XiamiUpdater.exe") continue;
                    File.Copy(f, cwd(fname), true);
                }
                catch (Exception e)
                {
                    File.AppendAllLines("XiamiUpdater.log", new string[] { e.Message, e.StackTrace });
                }
            }
            if (File.Exists(cwd("latest.txt")))
                File.Copy(cwd("latest.txt"), cwd("current.txt"), true);
            File.Delete(cwd("needs_update"));
            var info = new ProcessStartInfo(cwd("xiami.exe"));
            new Process { StartInfo = info }.Start();
        }
    }
}

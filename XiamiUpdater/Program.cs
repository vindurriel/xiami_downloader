using System;
using System.Diagnostics;
using System.IO;

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
            Console.ReadKey();
            if (!File.Exists(cwd("needs_update"))) return;
            var dir_src = cwd("latest", "xiami_downloader-master", "Jean_Doe.Output");
            if (!Directory.Exists(dir_src)) return;
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
                File.Copy(cwd("latest.txt"), cwd("current.txt"));
            File.Delete(cwd("needs_update"));
            var info = new ProcessStartInfo(cwd("xiami.exe"));
            new Process { StartInfo = info }.Start();
        }
    }
}

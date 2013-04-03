using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
namespace Jean_Doe.Common
{
    public static class RunProgramHelper
    {
        public static void RunProgram(string programName, string args)
        {
            try
            {
                var pi = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = args,
                    UseShellExecute = true,
                };
                new Process { StartInfo = pi }.Start();
            }
            catch (Exception) { }
        }
        public static Task<string> RunProgramGetOutput(string programName, string[] args)
        {
            try
            {
                var pi = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = string.Join(" ",args),
                    UseShellExecute = false,
                    RedirectStandardOutput=true,
                    RedirectStandardError=true,
                    CreateNoWindow = true,
                };
                var p=new Process { StartInfo = pi,EnableRaisingEvents=true };
                var tcs = new TaskCompletionSource<string>();
                p.Exited += async (s, e) =>
                {
                    var res = "";
                    if (p.ExitCode != 0)
                        res = await p.StandardError.ReadToEndAsync();
                    else
                        res = await p.StandardOutput.ReadToEndAsync();
                    tcs.SetResult(res);
                };
                p.Start();
                return tcs.Task;
            }
            catch (Exception e) {
                return null;
            }
        }
       
    }
}

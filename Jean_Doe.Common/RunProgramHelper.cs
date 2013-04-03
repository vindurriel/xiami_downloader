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
        public async static Task<string> RunProgramGetOutput(string programName, string[] args)
        {
            try
            {
                var pi = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = string.Join(" ",args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                var p=new Process { StartInfo = pi,EnableRaisingEvents=true };
                var tcs = new TaskCompletionSource<string>();
                p.Start();
                var res=await p.StandardOutput.ReadToEndAsync();
                return res;
            }
            catch (Exception e) {
                return null;
            }
        }

       
    }
}

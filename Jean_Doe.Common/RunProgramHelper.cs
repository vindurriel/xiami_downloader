using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
    }
}

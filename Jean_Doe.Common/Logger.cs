using System;
using System.IO;
namespace Jean_Doe.Common
{
    public class Logger
    {
        static object lockLog = new object();
        static readonly string logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("MMddHH") + ".log");
        public static void Error(Exception e)
        {
            lock (lockLog)
            {
                File.AppendAllText(logfile, string.Format("{0}\n{1}\n", e.Message, e.StackTrace));
            }
        }
    }
}

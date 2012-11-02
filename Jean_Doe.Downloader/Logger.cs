using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.Downloader
{
	public class Logger
	{
		static object lockLog=new object();
		static readonly string  logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("MMddHH")+".log");
		public static void Error(Exception e)
		{
			lock(lockLog)
			{
				File.AppendAllText(logfile, string.Format("{0}\n{1}\n", e.Message, e.StackTrace));
			}
		}
	}
}

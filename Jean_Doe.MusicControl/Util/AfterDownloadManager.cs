using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.MusicControl
{
	public static class AfterDownloadManager
	{
		public static void Register(string id,Action<string> action)
		{
			if(!dic.ContainsKey(id))
				dic[id]=new List<Action<string>>();
			dic[id].Add(action);
		}
		static object fireLock = new object();
		public static void Fire(string id, string arg)
		{
			lock(fireLock)
			{
				if(!dic.ContainsKey(id)) return;
				foreach(var action in dic[id])
				{
					try
					{
						action(arg);
					}
					catch
					{
					}
				}
				dic[id].Clear();
				dic.Remove(id);
			}
		}
		static Dictionary<string, List<Action<string>>> dic = new Dictionary<string, List<Action<string>>>();
	}
}

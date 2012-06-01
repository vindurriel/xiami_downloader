using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jean_Doe.HTMLAnalisys
{
	public static class URLDecoder
	{



		public static string URLDecode(string encodedurl)
		{
			int a2 = int.Parse(encodedurl.Substring(0, 1));
			string a3 = encodedurl.Substring(1);
			int a4 = (int)Math.Floor((double)a3.Length / (double)a2);
			int a5 = a3.Length % a2;
			List<string> a6 = new List<string>();
			while (a6.Count < a5)
			{
				a6.Add("");
			}
			int a7;
			for (a7 = 0; a7 < a5; a7++)
			{
				a6[a7] = a3.Substring((a4 + 1) * a7, a4 + 1);
			}
			a7 = a5;
			while (a6.Count < a2)
			{
				a6.Add("");
			}
			while (a7 < a2)
			{
				a6[a7] = a3.Substring(a4 * (a7 - a5) + (a4 + 1) * a5, a4);
				a7++;
			}
			string a8 = "";
			for (a7 = 0; a7 < a6[0].Length; a7++)
			{
				for (int a9 = 0; a9 < a6.Count; a9++)
				{
					if (a7 < a6[a9].Length)
					{
						a8 += a6[a9].Substring(a7, 1);
					}
				}
			}
			a8 = Uri.UnescapeDataString(a8);
			string a10 = "";
			for (a7 = 0; a7 < a8.Length; a7++)
			{
				if (a8.Substring(a7, 1) == "^")
				{
					a10 += "0";
				}
				else
				{
					a10 += a8.Substring(a7, 1);
				}
			}
			return a10.Replace('+', ' ');
		}
	}
}

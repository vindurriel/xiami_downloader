using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Jean_Doe.Common
{
    public static class StringHelper
    {
        static readonly HashSet<char> invalidChars;
        static StringHelper()
        {
            invalidChars = new HashSet<char>();
            foreach (var item in Path.GetInvalidFileNameChars())
            {
                invalidChars.Add(item);
                invalidChars.Add(';');
                invalidChars.Add('&');
            }
            string dump = "";
            foreach (char invalidChar in invalidChars)
                dump = dump + invalidChar;
        }
        /// <summary>
        /// returns a string valid for windows paths
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ValidPath(this string s, bool ignoreSep = false)
        {
            if (string.IsNullOrEmpty(s)) return null;
            string validFilePath = "";
            foreach (char c in s)
            {
                if (invalidChars.Contains(c))
                {
                    if (ignoreSep && c == Path.PathSeparator)
                        validFilePath += c;
                    else
                        validFilePath += '_';
                }

                else
                    validFilePath += c;
            }
            return validFilePath.Trim();
        }
        /// <summary>
        /// unescape uri, replace "+" with space
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EscapeUrl(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            try
            {
                s = Uri.EscapeUriString(s);
                s=s.Replace("/./","/");
            }
            catch
            {
            }
            return s;
        }
        public static int ToInt(this string s)
        {
            int res = 0;
            int.TryParse(s, out res);
            return res;
        }
        public static string UnescapeUrlWithSpace(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            try
            {
                s = Uri.UnescapeDataString(s)
                    .Replace(@"\/", "/")
                    .Replace(" ", "_");
            }
            catch
            {
            }
            return s;
        }
        /// <summary>
        /// Dicipher caesar matrix
        /// </summary>
        /// <param name="encodedurl"></param>
        /// <returns></returns>
        public static string DicipherCaesar(this string encodedurl)
        {
            if (string.IsNullOrEmpty(encodedurl)) return null;
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
        public static string gbk2utf8(this string raw)
        {
            var gbk = Encoding.GetEncoding(936);
            var bytes = gbk.GetBytes(raw);
            return Encoding.UTF8.GetString(bytes);
        }
        public static string utf82gbk(this string raw)
        {
            var gbk = Encoding.GetEncoding(936);
            var bytes = Encoding.UTF8.GetBytes(raw);
            return gbk.GetString(bytes);
        }
    }
}
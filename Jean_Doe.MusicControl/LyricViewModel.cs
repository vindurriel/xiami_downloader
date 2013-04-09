using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Jean_Doe.MusicControl
{
    public class LyricViewModel
    {
        public string Text { get; set; }
        public TimeSpan Time { get; set; }
        public static List<LyricViewModel> LoadLrcFile(string file)
        {
            var res = new List<LyricViewModel>();
            try
            {
                var f = File.ReadAllText(file);
                if (!f.Contains("]"))
                    foreach (var line in f.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries))
                    {
                        var item = new LyricViewModel
                        {
                            Time = TimeSpan.FromDays(1),
                            Text = line,
                        };
                        res.Add(item);
                    }
                else
                    foreach (var line in f.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries))
                    {
                        var strings = line.Split("]".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                        if (strings.Count() != 2) continue;
                        var t = TimeSpan.MaxValue;
                        if(TimeSpan.TryParseExact(strings[0].Replace("[", ""), @"mm\:ss\.ff", null,out t))
                        {
                            var item = new LyricViewModel
                            {
                                Time = t,
                                Text = strings[1],
                            };
                            res.Add(item);
                        }
                    }
            }
            catch (Exception e)
            {
            }
            res.Sort(comparer);
            return res;
        }
        static LyricComparer comparer = new LyricComparer();
    }
    public class LyricComparer : IComparer<LyricViewModel>
    {
        public int Compare(LyricViewModel x, LyricViewModel y)
        {
            return TimeSpan.Compare(x.Time, y.Time);
        }
    }
}

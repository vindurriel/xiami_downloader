using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace Jean_Doe.MusicControl
{
    public class LyricViewModel
    {
        public string Text { get; set; }
        public TimeSpan Time { get; set; }
        public static List<LyricViewModel> LoadLrcFile(string file)
        {
            var res = new List<LyricViewModel>();
            var pattern = new Regex("\\[(.*?)\\]");
            if (!System.IO.File.Exists(file)) return res;
            foreach (var line in File.ReadLines(file))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var m = pattern.Matches(line);
                var text = line;
                var time = TimeSpan.FromDays(1);
                if (m.Count > 0)
                {
                    text = pattern.Replace(line, "");
                    foreach (Match ma in m)
                    {
                        if (!TimeSpan.TryParseExact(ma.Groups[1].Value, @"mm\:ss\.ff", null, out time))
                            if (!TimeSpan.TryParseExact(ma.Groups[1].Value, @"mm\:ss", null, out time))
                                    text = ma.Groups[1].Value + text;
                        res.Add(new LyricViewModel
                            {
                                Time = time,
                                Text = text,
                            });
                    }
                }
                else
                    res.Add(new LyricViewModel
                          {
                              Time = time,
                              Text = text,
                          });
            }
            res = res.OrderBy(x => x.Time).ToList();
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

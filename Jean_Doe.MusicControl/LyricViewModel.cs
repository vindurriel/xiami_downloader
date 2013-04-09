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
                foreach (var line in File.ReadAllLines(file))
                {
                    var strings = line.Split("]".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Count() != 2) continue;
                    var item = new LyricViewModel
                    {
                        Time = TimeSpan.ParseExact(strings[0].Replace("[", ""), @"mm\:ss\.ff", null),
                        Text = strings[1],
                    };
                    res.Add(item);
                }
            }
            catch (Exception e)
            {
            }
            return res;
        }
    }
}

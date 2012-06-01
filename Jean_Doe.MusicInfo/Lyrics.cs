using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jean_Doe.MusicInfo
{
    public class Lyrics
    {
        private List<LyricsUnit> lyricsSet;

        public Lyrics()
        {
            lyricsSet = new List<LyricsUnit>();
        }

        public void AddLyrics(int timestamp, string lyrics)
        {
            lyricsSet.Add(new LyricsUnit
                                    {
                                        TimeStamp = timestamp,
                                        Lyrics = lyrics
                                    }
                                );
        }

        public void AddLyrics(LyricsUnit lunit)
        {
            lyricsSet.Add(lunit);
        }

        public void Clear()
        {
            lyricsSet.Clear();
        }

        public List<LyricsUnit> GetLyris()
        {
            return lyricsSet;
        }

        public List<LyricsUnit> GetSortedLyrics()
        {
            lyricsSet.Sort();
            return lyricsSet;
        }
        public static Lyrics LoadFromStr(string raw)
        {
            var res = new Lyrics();
            string[] lysList = raw.Split(new char[]
				{
					'\n'
				});
            var lrcRegex = new Regex("\\[(\\d+):(\\d+\\.\\d+)\\]");
            foreach (string t in lysList)
            {
                Match j = lrcRegex.Match(t);
                string lyric = "";
                List<int> times = new List<int>();
                int index = -1;
                while (j.Success)
                {
                    string minutes = j.Groups[1].Value;
                    string seconds = j.Groups[2].Value;
                    int time = (int)((double.Parse(minutes) * 60.0 + double.Parse(seconds)) * 1000.0);
                    times.Add(time);
                    index = j.Index + j.Groups[0].Value.Length;
                    j = j.NextMatch();
                }
                if (index != -1 && index != t.Length)
                {
                    lyric = t.Substring(index);
                }
                foreach (int time2 in times)
                {
                    res.AddLyrics(time2, lyric);
                }
            }
            return res;
        }
    }
}

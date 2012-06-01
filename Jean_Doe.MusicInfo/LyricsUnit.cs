using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jean_Doe.MusicInfo
{
	public class LyricsUnit : IComparable<LyricsUnit>
	{
		private int timeStamp = -1;
		public int TimeStamp
		{
			get
			{
				return timeStamp;
			}
			set
			{
				timeStamp = value;
			}
		}

		private string lyric = "";
		public string Lyrics
		{
			get
			{
				return lyric;
			}
			set
			{
				lyric = value;
			}
		}

		public int CompareTo(LyricsUnit unit)
		{
			return TimeStamp - unit.TimeStamp;
		}
	}
}

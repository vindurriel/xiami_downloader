using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    public class PlayList : IHandle<MsgRequestNextSong>
    {
        bool needsRefresh = false;
        public static void NeedsRefresh()
        {
            instance.needsRefresh = true;
        }
        public static PlayList instance = new PlayList();
        public PlayList()
        {
            Artwork.MessageBus.MessageBus.Instance.Subscribe(this);
        }
        public static bool Contains(SongViewModel s)
        {
            if (s == null || string.IsNullOrEmpty(s.Id)) return false;
            return instance.playlist.Any(x => x.Id == s.Id);
        }
        public static void Refresh(IEnumerable<SongViewModel> selSongs = null)
        {
            if (!instance.needsRefresh) return;
            instance.playlist.Clear();
            foreach (var item in selSongs)
            {
                instance.playlist.Add(item);
            };
            if (Global.AppSettings["PlayNextMode"] == "Random")
                instance.playlist.Shuffle();
            instance.needsRefresh = false;
        }
        List<SongViewModel> playlist = new List<SongViewModel>();
        public static IEnumerable<MusicViewModel> ItemSource { get { return instance.playlist; } }

        public void Handle(MsgRequestNextSong message)
        {
            if (SongViewModel.All.Count() == 0) return;
            if (playlist.Count == 0)
            {
                Refresh(SongViewModel.All.Where(x => x.Song.DownloadState == "complete"));
            }
            SongViewModel item = null;
            var now = SongViewModel.NowPlaying ?? SongViewModel.All.FirstOrDefault();
            var mode = EnumPlayNextMode.Random;
            Enum.TryParse(Global.AppSettings["PlayNextMode"], out mode);
            switch (mode)
            {
                case EnumPlayNextMode.Sequential:
                case EnumPlayNextMode.Random:
                    int i = playlist.IndexOf(now);
                    if (i == -1) i = playlist.Count - 1;
                    i = i == playlist.Count - 1 ? 0 : i + 1;
                    item = playlist.ElementAt(i);
                    break;
                case EnumPlayNextMode.Repeat:
                    item = now;
                    Mp3Player.CurrentTime = 0.0;
                    break;
                default:
                    break;
            }
            if (item == null) return;
            message.Next = item.Song.DownloadState == "complete" ? item.Song.FilePath : item.Song.UrlMp3;
            message.Id = item.Id;
        }
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Jean_Doe.Common;
using Jean_Doe.Downloader;

namespace Jean_Doe.MusicControl
{
    public class DownloadSongListControl : SongListControl
    {
        public DownloadSongListControl()
        {

        }
        public override void Load()
        {
            base.Load();
            PrepareDownload(Items.OfType<SongViewModel>());
        }
        public override void Add(SongViewModel song)
        {
            base.Add(song);
            PrepareDownload(song);
        }
        public static void PrepareDownload(IEnumerable<SongViewModel> list)
        {
            foreach (var item in list)
            {
                PrepareDownload(item);
            }
        }
        public static void PrepareDownload(SongViewModel item)
        {
            var nameBase = System.IO.Path.Combine(Global.BasePath, "cache", item.Id);
            if (!item.HasMp3)
            {
                var mp3 = nameBase + ".mp3";
                var d = new DownloaderMp3
                {
                    Info = new DownloaderInfo
                    {
                        Id = "mp3" + item.Id,
                        FileName = mp3,
                        Entity = item,
                        Tag = item.Id,
                    }
                };
                DownloadManager.Instance.Add(d);
            }
            if (!item.HasLrc)
            {
                var lrc = nameBase + ".lrc";
                var d = new DownloaderLrc
                {
                    Info = new DownloaderInfo
                    {
                        Id = "lrc" + item.Id,
                        FileName = lrc,
                        Tag = item.Id,
                        Entity = item
                    }
                };
                DownloadManager.Instance.Add(d);
            }
            if (!item.HasArt)
            {
                var art = System.IO.Path.Combine(
                    Global.BasePath, "cache",
                    item.AlbumId + ".art");
                var d = new DownloaderArt
                {
                    Info = new DownloaderInfo
                    {
                        Id = "art" + item.AlbumId,
                        FileName = art,
                        Entity = item,
                        Tag = item.Id,
                    }
                };
                var first = DownloadManager.Instance.GetDownloader(d.Info.Id) as DownloaderArt;
                if (first == null)
                    DownloadManager.Instance.Add(d);
                else
                {
                    first.ArtDownloaded += d.HandleDownloaded;
                }
            }
        }
    }
}

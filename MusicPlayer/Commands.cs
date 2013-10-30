using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using Jean_Doe.MusicControl;

namespace MusicPlayer
{
    internal class Commands
    {
        //public static void PrepareDownload(IEnumerable<SongViewModel> list)
        //{
        //    foreach(var item in list)
        //    {
        //        PrepareDownload(item);
        //    }
        //}
        //public static  void PrepareDownload(SongViewModel item)
        //{
        //    var nameBase = System.IO.Path.Combine(Global.BasePath, "cache", item.SongId);
        //    if(!item.HasXml)
        //    {
        //        var xml = nameBase + ".xml";
        //        var urlXml = XiamiUrl.UrlPlaylistByIdAndType(item.SongId, EnumXiamiType.Song);
        //        var d = new DownloaderManifest
        //        {
        //            Info = new DownloaderInfo
        //            {
        //                Id = "xml" + item.SongId,
        //                Entity = item,
        //                Url = urlXml,
        //                FileName = xml,
        //                Tag = item.SongId,
        //            }
        //        };
        //        DownloadManager.Instance.Add(d);
        //    }
        //    if(!item.HasMp3)
        //    {
        //        var mp3 = nameBase + ".mp3";
        //        var d = new DownloaderMp3
        //        {
        //            Info = new DownloaderInfo
        //            {
        //                Id = "mp3" + item.SongId,
        //                FileName = mp3,
        //                Entity = item,
        //                Tag = item.SongId,
        //            }
        //        };
        //        DownloadManager.Instance.Add(d);
        //    }
        //    if(!item.HasLrc)
        //    {
        //        var lrc = nameBase + ".lrc";
        //        var d = new DownloaderLrc
        //        {
        //            Info = new DownloaderInfo
        //            {
        //                Id = "lrc" + item.SongId,
        //                FileName = lrc,
        //                Tag = item.SongId,
        //                Entity = item
        //            }
        //        };
        //        DownloadManager.Instance.Add(d);
        //    }
        //    if(!item.HasArt)
        //    {
        //        var art = System.IO.Path.Combine(
        //            Global.BasePath, "cache",
        //            item.AlbumId + ".art");
        //        var d = new DownloaderArt
        //        {
        //            Info = new DownloaderInfo
        //            {
        //                Id = "art" + item.AlbumId,
        //                FileName = art,
        //                Entity = item,
        //                Tag = item.SongId,
        //            }
        //        };
        //        var first = DownloadManager.Instance.GetDownloader(d.Info.Id) as DownloaderArt;
        //        if(first == null)
        //            DownloadManager.Instance.Add(d);
        //        else
        //        {
        //            first.ArtDownloaded += d.HandleDownloaded;
        //        }
        //    }
        //}
    }
}

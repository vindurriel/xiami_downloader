﻿using Jean_Doe.Common;
using Jean_Doe.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.MusicControl
{
	public static class SongViewModelExtentions
	{
		public static void PrepareDownload(this SongViewModel item)
		{
			var nameBase = System.IO.Path.Combine(Global.BasePath, "cache", item.Id);
			AfterDownloadManager.Register("art" + item.AlbumId, x => {
				item.ImageSource = x;
				item.HasArt = true;
			});
			if(!item.HasArt)
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
						Priority=2,
						Url=item.UrlArt,
					}
				};
				DownloadManager.Instance.Add(d);
			}
			if(!item.HasMp3)
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
						Priority = 1,
						Url=item.UrlMp3,
					}
				};
				DownloadManager.Instance.Add(d);
			}
			if(!item.HasLrc)
			{
				var lrc = nameBase + ".lrc";
				var d = new DownloaderLrc
				{
					Info = new DownloaderInfo
					{
						Id = "lrc" + item.Id,
						FileName = lrc,
						Tag = item.Id,
						Entity = item,
						Url= item.UrlLrc,
						Priority=0,
					}
				};
				DownloadManager.Instance.Add(d);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using Jean_Doe.Downloader;
using System.Text.RegularExpressions;
namespace Jean_Doe.MusicControl
{
    public sealed class DownloaderLrc : Downloader.Downloader
    {
        public override async Task Download()
        {
			try
			{
				var item = Info.Entity as SongViewModel;
				if(item == null)
				{
					throw new Exception("song item is null");
				}
				if(string.IsNullOrEmpty(item.UrlLrc))
				{
					item.UrlLrc = await NetAccess.GetUrlLrc(item.Id);
				}
				Info.Url = item.UrlLrc;
				if(string.IsNullOrEmpty(Info.Url))
				{
					State = EnumDownloadState.Error;
					return;
				}
			}
			catch(Exception e)
			{
				NotifyError(e);
				return;
			}
            await base.Download();
        }
        public override void Process()
        {
            base.Process();
            try
            {
				var song = Info.Entity as SongViewModel;
				if(song == null)
				{
					throw new Exception("song is null");
				}
				var folder = Path.Combine(Global.AppSettings["DownloadFolder"], song.Dir);
				if(folder != null && !Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				var lrc = Path.Combine(folder, song.FileNameBase + ".lrc");
				var txt = File.ReadAllText(Info.FileName);
				var m = Regex.Match(txt, "\"msg\":\"([^\"]+?)\"");
				if(m.Success)
					txt = m.Groups[1].Value;
				File.WriteAllText(lrc, txt,Encoding.UTF8);
                song.HasLrc = true;
            }
            catch (Exception e)
            {
                NotifyError(e);
            }
        }
    }
}

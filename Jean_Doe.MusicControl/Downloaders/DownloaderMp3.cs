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
    public sealed class DownloaderMp3 : Downloader.Downloader
    {
        public override async Task Download()
        {
            if (!(Info.Entity is SongViewModel))
                return;
            Info.Url = (Info.Entity as SongViewModel).UrlMp3;
            await base.Download();
        }
		public override bool CanProcess
		{
			get
			{
				var song = Info.Entity as SongViewModel;
				return song != null && song.HasArt;
			}
		}
        public override void Process()
        {
            base.Process();
            try
            {
				var item = Info.Entity as SongViewModel;
				if(item == null) 
					throw new Exception("item is null");
				var folder = Path.Combine(Global.AppSettings["DownloadFolder"], item.Dir);
				if(folder != null && !Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				var mp3 = Path.Combine(folder, item.FileNameBase + ".mp3");
                File.Copy(Info.FileName, mp3, true);
				if(item.Song.WriteId3)
				{
					var id3 = new MusicInfo.MusicInfo(mp3)
					{
						Album = item.AlbumName,
						Artist = item.ArtistName,
						Title = item.Name,
						Id = item.Id,
						Cover = item.ImageSource
					};
					if(item.TrackNo > 0)
						id3.TrackNo = item.TrackNo.ToString();
					id3.Commit();
				}
                item.HasMp3 = true;
            }
            catch (Exception e)
            {
                NotifyError(e);
            }
        }
        protected override void OnProgressChanged()
        {
            base.OnProgressChanged();
            setProgress(Percent);
            string s = string.Format("{0}/{1}"
            , curBytes.ToReadableString()
            , totalBytes.ToReadableString()
            );
            NotifyState(s);
        }
        void setProgress(int percent)
        {
            MessageBus.Instance.Publish(new MsgDownloadProgressChanged { Id = Info.Tag, Percent = percent });
        }
    }
}

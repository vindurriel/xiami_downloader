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
        public override void Process()
        {
            base.Process();
            var item = Info.Entity as SongViewModel;
            if (item == null) return;
            var folder = Path.Combine(Global.AppSettings["DownloadFolder"], item.Dir);
            if (folder != null && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var mp3 = Path.Combine(folder, item.FileNameBase + ".mp3");
            try
            {
                File.Copy(Info.FileName, mp3, true);
                var id3 = new MusicInfo.MusicInfo(mp3)
                {
                    Album = item.AlbumName,
                    Artist = item.ArtistName,
                    Title = item.Name,
                    Id = item.Id,
                };
                if (item.TrackNo > 0)
                    id3.TrackNo = item.TrackNo.ToString();
                id3.Commit();
                item.HasMp3 = true;
            }
            catch (Exception e)
            {
                State = EnumDownloadState.Error;
                NotifyState(e.Message);
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
    public sealed class DownloaderLrc : Downloader.Downloader
    {
        public override async Task Download()
        {
            var item = Info.Entity as SongViewModel;
            if (item == null)
                return;
            if (string.IsNullOrEmpty(item.UrlLrc))
            {
                item.UrlLrc = await NetAccess.GetUrlLrc(item.Id);
            }
            Info.Url = item.UrlLrc;
            if (string.IsNullOrEmpty(Info.Url))
            {
                return;
            }
            await base.Download();
        }
        public override void Process()
        {
            base.Process();
            var song = Info.Entity as SongViewModel;
            if (song == null) return;
            var folder = Path.Combine(Global.AppSettings["DownloadFolder"], song.Dir);
            if (folder != null && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var lrc = Path.Combine(folder, song.FileNameBase + ".lrc");
            try
            {
                File.Copy(Info.FileName, lrc, true);
                song.HasLrc = true;
            }
            catch (Exception e)
            {
                State = EnumDownloadState.Error;
                NotifyState(e.Message);
            }
        }
    }
    public sealed class DownloaderArt : Downloader.Downloader
    {
        public override bool CanProcess
        {
            get
            {
                var song = Info.Entity as SongViewModel;
                return song != null && song.HasMp3;
            }
        }

        public override async Task Download()
        {
            if (!(Info.Entity is SongViewModel))
                return;
            Info.Url = (Info.Entity as SongViewModel).UrlArt;
            await base.Download();
        }
        protected override void OnDownloaded()
        {
            var item = Info.Entity as SongViewModel;
            if (item != null)
            {
                if (ArtDownloaded != null)
                    ArtDownloaded(this, new ArtDownloadedEventArgs { song = item });
                item.ImageSource = Info.FileName;
            }
            base.OnDownloaded();
        }
        public override void Process()
        {
            base.Process();
            var item = Info.Entity as SongViewModel;
            if (item == null) return;
            if (item.ImageSource == null)
                item.ImageSource = Info.FileName;
            var folder = Path.Combine(Global.AppSettings["DownloadFolder"], item.Dir);
            if (folder != null && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var mp3 = Path.Combine(folder, item.FileNameBase + ".mp3");
            try
            {
                var id3 = new MusicInfo.MusicInfo(mp3);
                id3.Cover = Info.FileName;
                id3.Commit();
                item.HasArt = true;
            }
            catch (Exception e)
            {
                State = EnumDownloadState.Error;
                NotifyState(e.Message);
            }
        }
        public void HandleDownloaded(object sender, ArtDownloadedEventArgs e)
        {
            if (e == null || e.song == null) return;
            var song = Info.Entity as SongViewModel;
            if (song == null) return;
            OnDownloaded();
        }
        public event EventHandler<ArtDownloadedEventArgs> ArtDownloaded;
    }
    public class ArtDownloadedEventArgs : EventArgs
    {
        public SongViewModel song;
    }
}

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
            var song = Info.Entity as SongViewModel;
            if (song == null)
                return;
            var s = await XiamiClient.GetDefault().Call_xiami_api("Songs.getTrackDetail",
                "id=" + song.Id,
                "device_id=android-307320b12d9283df",
                "quality=h");
            Info.Url = s.track_url;
            await base.Download();
        }

        public override void Process()
        {
            base.Process();
            try
            {
                var item = Info.Entity as SongViewModel;
                var folder = Path.Combine(Global.AppSettings["DownloadFolder"], item.Dir);
                if (folder != null && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var mp3 = Path.Combine(folder, item.FileNameBase + ".mp3");
                File.Copy(Info.FileName, mp3, true);
                if (item.Song.WriteId3)
                {
                    var id3 = TagLib.File.Create(mp3);
                    id3.Tag.Clear();
                    id3.Tag.Album = item.AlbumName;
                    id3.Tag.Performers = new string[] { item.ArtistName };
                    id3.Tag.AlbumArtists = new string[] { item.ArtistName };
                    id3.Tag.Title = item.Name;
                    try
                    {
                        id3.Tag.Lyrics = File.ReadAllText(Path.Combine(folder, item.FileNameBase + ".lrc"));
                    }
                    catch (Exception)
                    {
                    }
                    id3.Tag.Comment = string.Join(" ", new[] { item.Id, item.ArtistId, item.AlbumId });
                    if (item.TrackNo > 0)
                        id3.Tag.Track = (uint)item.TrackNo;
                    if (item.ImageSource != null)
                    {
                        try
                        {
                            id3.Tag.Pictures = new TagLib.IPicture[] {
                                new TagLib.Picture(Path.Combine(Global.BasePath, "cache", string.Format("album_{0}.jpg", item.AlbumId))) };
                        }
                        catch (Exception e)
                        {
                            NotifyError(e);
                            Common.Logger.Error(e);
                        }
                    }
                    id3.Save();
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

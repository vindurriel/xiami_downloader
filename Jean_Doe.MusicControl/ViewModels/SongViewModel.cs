using System;
using System.IO;
using System.Windows.Controls;
using Jean_Doe.Common;
using System.Collections.Generic;
namespace Jean_Doe.MusicControl
{
    public class SongViewModel : MusicViewModel, IHasArtist, IHasAlbum
    {
        #region Properties
        double playTimes = 0;
        public double PlayTimes
        {
            get { return playTimes; }
            set { playTimes = value; Notify("PlayTimes"); }
        }
        DateTime date = DateTime.Now;
        public DateTime Date
        {
            get { return date; }
            set { date = value; Notify("Date"); }
        }
        public bool HasLrc
        {
            get { return song.HasLrc; }
            set { song.HasLrc = value; Notify("HasLrc"); }
        }
        public bool InFav
        {
            get { return song.InFav; }
            set { song.InFav = value; Notify("InFav"); }
        }
        public bool HasMp3
        {
            get { return song.HasMp3; }
            set { song.HasMp3 = value; Notify("HasMp3"); }
        }
        public bool HasArt
        {
            get { return song.HasArt; }
            set { song.HasArt = value; Notify("HasArt"); }
        }

        public override string Name { get { return song.Name; } }
        public string AlbumName { get { return song.AlbumName; } }
        public string ArtistName { get { return song.ArtistName; } }
        public string ArtistId { get { return song.ArtistId; } }
        public string AlbumId { get { return song.AlbumId; } }
        public string UrlMp3 { get { return song.UrlMp3; } }
        public string UrlArt { get { return song.UrlArt; } }
        public string UrlLrc { get { return song.UrlLrc; } set { song.UrlLrc = value; } }
        public int TrackNo { get { return song.TrackNo; } set { song.TrackNo = value; Notify("TrackNo"); } }
        string lyric = null;

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; Notify("Status"); }
        }
        private double percent;
        public double Percent
        {
            get { return percent; }
            set { percent = value; Notify("Percent"); }
        }
        #endregion
        #region update methods

        public void SetProgress(int i)
        {
            if (i >= 100)
            {
                i = 100;
            }
            Percent = i / 100.0;
        }
        #endregion
        public void Open()
        {
            if (!CanOpen) return;
            var filename = System.IO.Path.Combine(Global.AppSettings["DownloadFolder"], Dir, FileNameBase + ".mp3");
            if (File.Exists(filename))
            {
                RunProgramHelper.RunProgram("explorer.exe", string.Format("/select, \"{0}\"", filename));
            }
        }
        public static void ClearFav()
        {
            foreach (var item in cache)
            {
                item.Value.InFav = false;
            }
        }
        static Dictionary<string, SongViewModel> cache = new Dictionary<string, SongViewModel>();
        public static SongViewModel Get(Song song)
        {
            var id = song.Id;
            if (!cache.ContainsKey(id))
            {
                cache[id] = new SongViewModel(song);
            }
            if (song.InFav)
            {
                cache[id].InFav = true;
            }
            return cache[id];
        }
        public SongViewModel(Song song)
            : base(song)
        {
            this.song = song;
            TypeImage = ImageSource = "/Jean_Doe.MusicControl;component/Resources/song.png";
            var art = System.IO.Path.Combine(Global.BasePath, "cache", AlbumId + ".art");
            if (File.Exists(art))
                ImageSource = art;
            else if (File.Exists(song.FilePath))
            {
                
            }
        }
        Song song;
        public Song Song
        {
            get
            {
                return song;
            }
        }
        public string Dir
        {
            get
            {
                return ComposePath(true);
            }
        }
        public string FileNameBase
        {
            get
            {
                return ComposePath(false);
            }
        }
        string ComposePath(bool isFolder)
        {
            var pattern = isFolder ? Global.AppSettings["FolderPattern"] : Global.AppSettings["SongnamePattern"];
            var tags = new string[] { "ArtistName", "AlbumName", "Id", "Name" };
            foreach (var tag in tags)
            {
                System.Reflection.PropertyInfo pi = GetType().GetProperty(tag);
                var value = pi.GetValue(this, null);
                if (value != null)
                    pattern = pattern.Replace("%" + tag, value.ToString().ValidPath(ignoreSep: isFolder));
            }
            return pattern;
        }
        public bool CanOpen
        {
            get
            {
                return HasMp3 && HasArt;
            }
        }
        public bool Done
        {
            get
            {
                return HasMp3;
            }
        }
    }
}

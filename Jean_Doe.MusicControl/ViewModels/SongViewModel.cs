using System;
using System.IO;
using System.Windows.Controls;
using Jean_Doe.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
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
        private static SongViewModel nowPlaying;

        public static SongViewModel NowPlaying
        {
            get { return nowPlaying; }
            set { nowPlaying = value; }
        }

        public DateTime Date
        {
            get { return date; }
            set { date = value; Notify("Date"); }
        }
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public bool HasLrc
        {
            get { return song.HasLrc; }
            set { song.HasLrc = value; Notify("HasLrc"); }
        }
        public new bool InFav
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
            var filename = song.FilePath;
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
        public static SongViewModel GetId(string id)
        {
            if (string.IsNullOrEmpty(id) || !cache.ContainsKey(id)) return null;
            return cache[id];
        }
        public static SongViewModel Get(Song song)
        {
            var id = song.Id;
            if (!cache.ContainsKey(id))
            {
                cache[id] = new SongViewModel(song);
                isDirty = true;
            }
            if (song.InFav)
            {
                cache[id].InFav = true;
            }
            if (!string.IsNullOrEmpty(song.Description))
            {
                cache[id].Description = song.Description;
            }
            return cache[id];
        }
        public static SongViewModel[] All { get { return cache.Values.ToArray(); } }
        static DispatcherTimer timer = new DispatcherTimer();
        static SongViewModel()
        {
            timer.Tick += OnTimerTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        static void OnTimerTick(object sender, EventArgs e)
        {
            Save();
        }
        static bool isLoaded = false;
        public static void Load()
        {
            cache.Clear();
            PersistHelper.Load<Song>()
                .ForEach(x => Get(x));
            isLoaded = true;
        }
        public static bool isDirty = false;
        public static bool CanSave { get; set; }
        public static void RequestSave(SongViewModel s)
        {
            if (!isLoaded) return;
            PersistHelper.Save(new[] { s.Song });
        }
        public static void Save()
        {
            if (!isLoaded) return;
            if (!CanSave) return;
            if (!isDirty) return;
            var songs = cache.Values
                .Where(x => x.song.DownloadState != null)
                .Select(x => x.song)
                .ToArray();
            PersistHelper.Save(songs);
            isDirty = false;
        }
        public static bool Remove(string id)
        {
            isDirty = true;
            return cache.Remove(id);
        }
        public SongViewModel(Song song)
            : base(song)
        {
            this.song = song;
            TypeImage = "\xE189";
            ImageManager.Get(string.Format("{0}.art", song.AlbumId), song.Logo, ApplyLogo);
        }
        public override string Description
        {
            get
            {
                return Song.Description;
            }
            set
            {
                Song.Description = value;
                Notify("Description");
                Notify("HasDetail");
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
                return true;
            }
        }
    }
}

using System.IO;
using System.Windows.Controls;
using Jean_Doe.Common;
namespace Jean_Doe.MusicControl
{
    public class SongViewModel : MusicViewModel, IHasArtist,IHasAlbum
    {
        #region Properties
        double playTimes = 0;
        public double  PlayTimes
        {
            get { return playTimes; }
            set { playTimes = value; Notify("PlayTimes"); }
        }
        public bool HasLrc
        {
            get { return song.HasLrc; }
            set { song.HasLrc = value; Notify("HasLrc"); }
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
        public int TrackNo { get { return song.TrackNo; } set { song.TrackNo = value; } }


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
            if(i >= 100)
            {
                i = 100;
            }
            Percent = i / 100.0;
        }
        #endregion
        public void Open()
        {
            if(!CanOpen) return;
            var filename = System.IO.Path.Combine(Global.AppSettings["DownloadFolder"], Dir, FileNameBase + ".mp3");
            if(File.Exists(filename))
            {
                RunProgramHelper.RunProgram("explorer.exe", string.Format("/select, \"{0}\"", filename));
            }
        }
        public SongViewModel(Song song)
            : base(song)
        {
            this.song = song;
            if(HasArt)
            {
                ImageSource = System.IO.Path.Combine(Global.BasePath, "cache", AlbumId + ".art");
            }
        }
        #region 试听
        private WebBrowser playContent;
        public WebBrowser PlayContent
        {
            get { return playContent; }
            set { playContent = value; Notify("PlayContent"); }
        }
        public void Play()
        {
            PlayContent = new WebBrowser();
            PlayContent.Width = 257;
            PlayContent.Height = 33;
            var uriPlay = string.Format("http://www.xiami.com/widget/0_{0}/singlePlayer.swf", Id);
            PlayContent.Navigate(uriPlay);
            isPlaying = true;
        }
        public void Stop()
        {
            if(PlayContent == null) return;
            PlayContent.Navigated += (s, e) =>
            {
                PlayContent.Dispose();
                PlayContent = null;
                isPlaying = false;
            };
            PlayContent.Navigate("about:blank");
        }
        bool isPlaying = false;
        public void TogglePlay()
        {
            if(isPlaying) Stop();
            else Play();
        }
        #endregion
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
            foreach(var tag in tags)
            {
                System.Reflection.PropertyInfo pi = GetType().GetProperty(tag);
                var value = pi.GetValue(this, null);
                if(value != null)
                    pattern = pattern.Replace("%" + tag, value.ToString().ValidPath(ignoreSep: isFolder));
            }
            return pattern;
        }
        public bool CanOpen
        {
            get
            {
                return HasMp3  && HasArt;
            }
        }
        public bool AllDone
        {
            get
            {
                return HasMp3 && HasArt && HasLrc;
            }
        }
    }
}

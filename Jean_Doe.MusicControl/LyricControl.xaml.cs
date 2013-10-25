using Jean_Doe.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// LyricControl.xaml 的交互逻辑
    /// </summary>
    public partial class LyricControl
    {
        public LyricControl()
        {
            InitializeComponent();
            ItemsSource = source;
            Global.ListenToEvent("ShowLyric", OnShowLyric);
        }
        void OnShowLyric(string s)
        {
            if (s == "1")
            {
                Mp3Player.SongChanged -= OnMp3PlayerSongChanged;
                Mp3Player.TimeChanged -= OnMp3PlayerTimeChanged;
                Mp3Player.SongChanged += OnMp3PlayerSongChanged;
                Mp3Player.TimeChanged += OnMp3PlayerTimeChanged;
            }
            else
            {
                source.Clear();
                Mp3Player.SongChanged -= OnMp3PlayerSongChanged;
                Mp3Player.TimeChanged -= OnMp3PlayerTimeChanged;
            }
        }
        bool isworking;
        void OnMp3PlayerTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (timelist == null || timelist.Length == 0) return;
            var t = e.Current;
            if (isworking) return;
            isworking = true;
            var i = binarySearch(timelist, t, 0, timelist.Length - 1);
            isworking = false;
            if (i == -1) return;
            UIHelper.RunOnUI(() =>
            {
                SelectedIndex = i;
                this.ScrollToCenterOfView(SelectedItem);
            });
        }
        static int binarySearch(double[] list, double t, int l, int h)
        {
            while (l <= h)
            {
                int m = (l + h) / 2;
                double cur = list[m];
                if (t > cur)
                    l = m + 1;
                else if (t < cur)
                    h = m - 1;
                else
                {
                    h = m;
                    break;
                }
            }
            return h;
        }

        ObservableCollection<LyricViewModel> source = new ObservableCollection<LyricViewModel>();
        double[] timelist = null;
        void OnMp3PlayerSongChanged(object sender, SongChangedEventArgs e)
        {
            SongViewModel svm = SongViewModel.GetId(e.Id);
            if (svm == null) return;
            var lrcPath = Path.Combine(Global.AppSettings["DownloadFolder"], svm.FileNameBase + ".lrc");
            if (!File.Exists(lrcPath))
                lrcPath = Path.Combine(Global.BasePath, "cache", e.Id + ".lrc");
            if (!File.Exists(lrcPath))
            {
                Task.Run(async () =>
                {
                    var url = svm.Song.UrlLrc;
                    if (string.IsNullOrEmpty(url))
                    {
                        var json = await NetAccess.Json(XiamiUrl.url_song, "id", svm.Id);
                        if (json.song != null)
                            json = json.song;
                        url = MusicHelper.Get(json, "lyric", "song_lrc");
                        if (string.IsNullOrEmpty(url))
                            return;
                    }
                    string lrcText = await Http.Get(url, null);
                    File.WriteAllText(lrcPath, lrcText);
                    var f = LyricViewModel.LoadLrcFile(lrcPath);
                    UIHelper.RunOnUI(() =>
                    {
                        source.Clear();
                        foreach (var item in f)
                        {
                            source.Add(item);
                        }
                        timelist = source.Select(x => x.Time.TotalMilliseconds).ToArray();
                    });
                });
                return;
            }
            var s = LyricViewModel.LoadLrcFile(lrcPath);
            source.Clear();
            foreach (var item in s)
            {
                source.Add(item);
            }
            timelist = source.Select(x => x.Time.TotalMilliseconds).ToArray();
        }
    }
}

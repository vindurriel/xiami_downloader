using Jean_Doe.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
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
            var lrcPath = Path.Combine(Global.BasePath, "cache", e.Id + ".lrc");
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

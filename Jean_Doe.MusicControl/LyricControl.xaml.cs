﻿using Jean_Doe.Common;
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
            Mp3Player.SongChanged += OnMp3PlayerSongChanged;
            Mp3Player.TimeChanged += OnMp3PlayerTimeChanged;
        }
        bool isworking;
        void OnMp3PlayerTimeChanged(object sender, Mp3Player.TimeChangedEventArgs e)
        {
            if (timelist == null || timelist.Length == 0) return;
            var t = e.Current;
            if (isworking) return;
            isworking = true;
            var i = binarySearch(timelist, t.TotalMilliseconds, 0, timelist.Length - 1);
            isworking = false;
            if (i == -1) return;
            UIHelper.RunOnUI(() =>
            {
                SelectedIndex = i;
                this.ScrollToCenterOfView(SelectedItem);
            });
        }
        public static int binarySearch(double[] list, double t, int l, int h)
        {
            if (list.Length == 0) return -1;
            if (list[0] > t) return -1;
            if (list[0] == t) return 0;
            if (list[list.Length - 1] <= t) return list.Length - 1;
            if (h - l == 1)
                return l;
            if (h == l)
            {
                if (list[h] > t)
                    return h - 1;
                return h;
            }
            int m = (l + h) / 2;
            double cur = list[m];
            if (cur > t)
                return binarySearch(list, t, l, m - 1);
            if (cur < t)
                return binarySearch(list, t, m + 1, h);
            return m;
        }

        ObservableCollection<LyricViewModel> source = new ObservableCollection<LyricViewModel>();
        double[] timelist = null;
        void OnMp3PlayerSongChanged(object sender, Mp3Player.SongChangedEventArgs e)
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
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Hardcodet.Wpf.TaskbarNotification;
using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :
        INotifyPropertyChanged,
        IHandle<MsgSearchStateChanged>,
        IHandle<string>,
        IHandle<MsgUpdateReady>,
        IHandle<MsgChangeWindowState>
    {
        TaskbarIcon trayIcon;
        FrameworkElement lastPage;
        private int page;
        public int Page
        {
            get { return page; }
            set
            {
                if (value > headingSource.Count - 1) value = 0;
                bool isLeft = value > page;
                page = value;

                foreach (var header in headers.Children.OfType<ToggleButton>())
                {
                    header.IsChecked = header.Tag.ToString() == page.ToString();
                }
                if (contents.Children.Count > page)
                {
                    var content = contents.Children[page] as FrameworkElement;
                    if (content != null)
                        showPage(content);
                }
                ActionBarService.ContextName = page.ToString();
                heading.DataContext = headingSource[page];
                if (page < lists.Count)
                    list_count.DataContext = lists[page];
                else
                    list_count.DataContext = null;
            }
        }
        void showPage(FrameworkElement content, bool isLeft = true)
        {
            if (content == lastPage) return;
            var slideInName = isLeft ? "SlideInLeft" : "SlideInRight";
            var slideOut = FindResource("SlideOut") as Storyboard;
            var slideIn = FindResource(slideInName) as Storyboard;
            if (lastPage != null)
            {
                slideOut.Begin(lastPage, true);
            }
            slideIn.Begin(content, true);
            lastPage = content;
        }

        public MainWindow()
        {
            Global.LoadSettings();
            Global.ListenToEvent("EnableMagnet", SetEnableMagnet);
            Global.ListenToEvent("ColorSkin", SetColorSkin);
            Global.ListenToEvent("Theme", SetTheme);
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            InitializeComponent();
            Global.ListenToEvent("PlayNextMode", SetPlayNextMode);
            Artwork.DataBus.DataBus.Set("MainWindow", this);
            DataContext = this;
            MessageBus.Instance.Subscribe(this);
            loadSongLists();
            ActionBarService.RegisterActionBar(this.charmBar);
            Artwork.DataBus.DataBus.Set("list_download", list_download);
            Mp3Player.SongChanged += OnMp3PlayerSongChanged;
            Global.ListenToEvent("TitleMarquee", SetTitleMarquee);
            btn_sync_left.Click += btn_sync_left_Click;
            btn_sync_right.Click += btn_sync_right_Click;
            new MusicSliderConnector(slider);
            lists = new List<SongListControl> { 
                list_search,
                list_download,
                list_complete,
            };
            foreach (var item in lists)
            {
                item.PropertyChanged += songlist_PropertyChanged;
                item.ListView.SelectionChanged += ListView_SelectionChanged;
            }
            content_mask.MouseLeftButtonUp += btn_toggle_detail_Click;
            info_now.MouseLeftButtonUp += btn_toggle_detail_Click;
            headingSource = new List<CharmAction>
                {
                    new CharmAction("在线", "\xE11a", (a,b)=>Page=0),
                    new CharmAction("下载", "\xE118", (a,b)=>Page=1),
                    new CharmAction("本地", "\xE142", (a,b)=>Page=2),
                    new CharmAction("配置", "\xE115", (a,b)=>Page=3),
                };
        }


        private void SetPlayNextMode(string s)
        {
            string c = "顺";
            string tooltip = "顺序播放";
            switch (s)
            {
                case "Random":
                    c = "随";
                    tooltip = "随机播放";
                    break;
                case "Repeat":
                    c = "单";
                    tooltip = "单曲循环";
                    break;
                default:
                    break;
            }
            btn_select_now_playing.Content = c;
            btn_select_now_playing.ToolTip = "播放模式:" + tooltip;
        }
        List<SongListControl> lists;
        bool isSyncingSelection = false;
        void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSyncingSelection) return;
            isSyncingSelection = true;
            var cur = currentList;
            foreach (var l in lists)
            {
                if (l == cur) continue;
                l.ListView.SelectedItems.Clear();
                foreach (var item in cur.SelectedItems)
                {
                    l.ListView.SelectedItems.Add(item);
                }
            }
            isSyncingSelection = false;
        }
        SongListControl currentList
        {
            get
            {
                try
                {
                    return lists[page];
                }
                catch
                {
                }
                return list_complete;
            }
        }
        void songlist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectCount") return;
            refreshPlause();
        }
        void refreshPlause()
        {
            string id = null;
            var song = currentList.SelectedSongs.FirstOrDefault();
            if (song != null)
                id = song.Id;
            btn_plause.Content = Mp3Player.GetPlayOrPause(id) ? "\xE102" : "\xE103";
        }
        void btn_sync_right_Click(object sender, RoutedEventArgs e)
        {
            if (this.webcontrol != null)
            {
                var query = searchKeywordControl.Key;
                var m = new System.Text.RegularExpressions.Regex("(.+?):(\\d+)").Match(query);
                if (m.Success)
                {
                    webcontrol.Navigate(string.Format("{0}/model/{1}_{2}?theme={3}", Global.AppSettings["url_nest"], m.Groups[1], m.Groups[2], theme));
                }
                else
                {
                    var song = this.list_search.SelectedSongs.FirstOrDefault();
                    if (song != null)
                    {
                        webcontrol.Navigate(string.Format("{0}/model/{1}_{2}?theme={3}", Global.AppSettings["url_nest"], "song", song.Id, theme));
                    }
                }
            }
        }

        async void btn_sync_left_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search(webcontrol_entity);
        }

        void OnMp3PlayerSongChanged(object sender, SongChangedEventArgs e)
        {
            if (SongViewModel.NowPlaying != null)
            {
                SongViewModel.NowPlaying.IsNowPlaying = false;
            }
            SongViewModel.NowPlaying = SongViewModel.GetId(e.Id);
            if (SongViewModel.NowPlaying != null)
            {
                Task.Run(() =>
                {
                    if (!Mp3Player.IsPlaying) return;
                    var id = SongViewModel.NowPlaying.Id;
                    System.Threading.Thread.Sleep(10000);
                    if (SongViewModel.NowPlaying.Id != id) return;
                    NetAccess.Json(XiamiUrl.url_playlog, "uid", Global.AppSettings["xiami_uid"], "id", id);
                    // XiamiClient.GetDefault().Call_xiami_api("Playlog.add",
                    //    "id=" + SongViewModel.NowPlaying.Id,
                    //    "time=" + XiamiClient.DateTimeToUnixTimestamp(DateTime.Now).ToString(),
                    //    "type=20"
                    //);
                });
                SongViewModel.NowPlaying.IsNowPlaying = true;
                var now = SongViewModel.NowPlaying;
                info_now.Visibility = Visibility.Visible;
                part_nowPlaying.DataContext = now;
                Title = string.Format("{0} - {1}      ", now.Name, now.ArtistName);
                counter = 0;
                if (trayIcon != null && Global.AppSettings["ShowNowPlaying"] != "0" && Mp3Player.IsPlaying)
                {
                    balloonTip = new MyBalloonTip();
                    balloonTip.ViewModel = now;
                    trayIcon.ShowCustomBalloon(balloonTip, PopupAnimation.Fade, 3000);
                }
            }
            ActionBarService.Refresh();
            refreshPlause();
            currentList.PerformAction("选中正在播放");
        }
        long counter;
        void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            if (Title.Length < 12) return;
            if (counter % 10 == 9)
            {
                Title = Title.Substring(1) + Title.Substring(0, 1);
            }
            counter += 1;
        }
        MyBalloonTip balloonTip;
        void OnTrayIconClick(object sender, EventArgs e)
        {
            Handle(new MsgChangeWindowState { State = EnumChangeWindowState.Minimized });
        }
        void initActionBar()
        {
            //ActionBarService.RegisterContext("1", userPage, "IsLoggedIn");
            ActionBarService.RegisterContext("0", list_search, "SelectCount");
            ActionBarService.RegisterContext("1", list_download, "SelectCount");
            ActionBarService.RegisterContext("2", list_complete, "SelectCount");
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs a)
        {
            initActionBar();
            var h = new WindowInteropHelper(this);
            h.EnsureHandle();
            try
            {
                trayIcon = new TaskbarIcon();
                var iconStream = App.GetResourceStream(new Uri("pack://application:,,/Resources/icon.ico")).Stream;
                trayIcon.Visibility = Visibility.Visible;
                trayIcon.TrayMouseDoubleClick += OnTrayIconClick;
                trayIcon.Icon = new System.Drawing.Icon(iconStream);
            }
            catch
            {
            }
            watcher = CreateWatcher();
            //tag control events
            foreach (var item in headers.Children)
            {
                var header = item as ToggleButton;
                if (header != null)
                    header.Click += (s, e) =>
                    {
                        var hd = (s as ToggleButton);
                        hd.IsChecked = true;
                        Page = int.Parse(hd.Tag.ToString());
                    };

            }
            //load last window position
            var rect = Rect.Parse(Global.AppSettings["WindowPos"]);
            if (rect.Width * rect.Height != 0)
            {
                Left = rect.Left; Top = rect.Top; Width = rect.Width; Height = rect.Height;
            }
            if (Left < 0) Left = 0;
            if (Top < 0) Top = 0;
            //load last active page
            var lastPage = int.Parse(Global.AppSettings["ActivePage"]);
            Page = lastPage;

            WebBrowserOverlay wbo = new WebBrowserOverlay(webBrowserPlaceHolder);
            webcontrol = wbo.WebBrowser;
            webcontrol.Navigated += (s, e) =>
            {
                var re = new System.Text.RegularExpressions.Regex(@"/model/(.+?)_(\d+)");
                var m = re.Match(e.Uri.ToString());
                if (m.Success)
                {
                    var text = string.Format("{0}:{1}", m.Groups[1], m.Groups[2]);
                    webcontrol_entity = text;
                    //Clipboard.SetData(DataFormats.Text, text);
                }
            };
            webcontrol.Navigate(string.Format("{0}/model/artist_1508?theme={1}", Global.AppSettings["url_nest"], theme));
        }
        string webcontrol_entity;
        WebBrowser webcontrol;
        private void loadSongLists()
        {
            SongViewModel.Load();
            list_download.SavePath = "download";
            list_complete.SavePath = "complete";
            list_download.Load();
            list_complete.Load();
        }

        void SetEnableMagnet(string on)
        {
            if (on == "1")
            {
                this.EnableMagnet();
                this.SetMagnetBorder(10);
            }
            else
            {
                this.DisableMagnet();
            }
        }
        void SetTitleMarquee(string s)
        {
            if (s == "1")
            {
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
                CompositionTarget.Rendering += OnCompositionTargetRendering;
            }
            else
            {
                if (SongViewModel.NowPlaying != null)
                    Title = string.Format("{0} - {1}      ", SongViewModel.NowPlaying.Name, SongViewModel.NowPlaying.ArtistName);
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
            }
        }
        string theme;
        void SetTheme(string s)
        {
            theme = s.StartsWith("#333") ? "dark" : "light";
            var colors = s.Split(" ".ToCharArray());
            App.Current.Resources["darkBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[0]) };
            App.Current.Resources["lightBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[1]) };
            App.Current.Resources["selectBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[2]) };
        }
        void SetColorSkin(string s)
        {
            App.Current.Resources["skinBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(s) };
        }



        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Mp3Player.Exit();
            trayIcon.Dispose();
            SongViewModel.Save();
            Global.AppSettings["ActivePage"] = Page.ToString();
            Global.AppSettings["WindowPos"] = new Rect(Left, Top, ActualWidth, ActualHeight).ToString();
            Global.SaveSettings();
        }


        void btn_move_MouseDown(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        public void Handle(MsgSearchStateChanged message)
        {
            switch (message.State)
            {
                case EnumSearchState.Started:
                    UIHelper.RunOnUI(() =>
                    {
                        Page = 2;
                    });
                    MessageBus.Instance.Publish(new MsgSetBusy(this, true));
                    break;
                case EnumSearchState.Cancelling:
                case EnumSearchState.Finished:
                    UIHelper.RunOnUI(() =>
                    {
                        Page = 2;
                    });
                    MessageBus.Instance.Publish(new MsgSetBusy(this, false));
                    break;
                default:
                    break;
            }
        }
        public void Handle(string message)
        {
        }
        public void Handle(MsgChangeWindowState message)
        {
            switch (message.State)
            {
                case EnumChangeWindowState.Close:
                    Close();
                    break;
                case EnumChangeWindowState.Maximized:
                    var w = SystemParameters.PrimaryScreenWidth;
                    var h = SystemParameters.PrimaryScreenHeight;
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    else
                    {
                        WindowState = WindowState.Maximized;
                    }
                    break;
                case EnumChangeWindowState.Minimized:
                    if (WindowState == WindowState.Normal)
                    {
                        WindowState = WindowState.Minimized;
                    }
                    else
                        WindowState = WindowState.Normal;
                    break;
                default:
                    break;
            }
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btn_restart_Click(object sender, RoutedEventArgs e)
        {
            Global.AppSettings["UpdateInfo"] = "";
            this.Closed += OnClosed;
            Close();
        }

        void OnClosed(object sender, EventArgs e)
        {
            RunProgramHelper.RunProgram("xiami.exe", "");
        }
        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            Handle(new MsgChangeWindowState { State = EnumChangeWindowState.Minimized });
        }

        public void Handle(MsgUpdateReady message)
        {
            UIHelper.RunOnUI(() => btn_restart.Visibility = Visibility.Visible);
        }

        private void btn_select_now_playing_Click(object sender, RoutedEventArgs e)
        {
            int mode = (int)Enum.Parse(typeof(EnumPlayNextMode), Global.AppSettings["PlayNextMode"]);
            mode = (mode + 1) % 3;
            var m = (EnumPlayNextMode)mode;
            Global.AppSettings["PlayNextMode"] = m.ToString();
        }

        private void btn_plause_Click(object sender, RoutedEventArgs e)
        {
            currentList.PerformAction("播放/暂停");
        }
        private void btn_toggle_detail_Click(object sender, RoutedEventArgs e)
        {
            bool flag = part_nowPlaying.Height == 60;
            var s = (this.TryFindResource(flag ? "SlideUp" : "SlideDown") as Storyboard);
            (s.Children[0] as DoubleAnimation).To = flag ? content_mask.ActualHeight : 60;
            content_mask.IsHitTestVisible = flag;
            info_now.IsHitTestVisible = !flag;
            s.Begin();
        }
        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            currentList.PerformAction("下一首");
        }
        static FileSystemWatcher watcher;
        FileSystemWatcher CreateWatcher()
        {
            using (var s = File.CreateText(Path.Combine(Global.BasePath, "a.txt"))) { }
            var f = new FileSystemWatcher(Global.BasePath, "a.txt") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
            f.Changed += f_Changed;
            return f;
        }
        void f_Changed(object sender, FileSystemEventArgs e)
        {
            while (true)
            {
                try
                {
                    using (var s = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                    }
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            System.Media.SystemSounds.Beep.Play();
            var cmd = File.ReadAllText(e.FullPath);
            if (cmd == "next")
                UIHelper.RunOnUI(() => btn_next_Click(null, null));
            else if (cmd == "pause")
                UIHelper.RunOnUI(() => btn_plause_Click(null, null));
            UIHelper.RunOnUI(() => ActionBarService.Refresh());
        }
        List<CharmAction> headingSource;
        private void header_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cm = new ContextMenu();
            cm.PreviewMouseLeftButtonUp += (s, a) => (s as ContextMenu).IsOpen = false;
            var current = heading.DataContext as CharmAction;
            cm.ItemsSource = headingSource.Where(x => x != current);
            cm.Placement = PlacementMode.Bottom;
            cm.PlacementTarget = sender as UIElement;
            cm.StaysOpen = false;
            cm.IsOpen = true;
        }
    }
}

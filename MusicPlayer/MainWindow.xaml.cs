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
                if (value == page) return;
                bool isLeft = value > page;
                page = value;
                foreach (var header in headers.Children.OfType<ToggleButton>())
                {
                    header.IsChecked = header.Tag.ToString() == page.ToString();
                }
                if (contents.Children.Count > page - 1)
                {
                    var content = contents.Children[page - 1] as FrameworkElement;
                    if (content != null)
                        showPage(content, isLeft);
                }
                ActionBarService.ContextName = page.ToString();
            }
        }
        void showPage(FrameworkElement content, bool isLeft = true)
        {
            if (content == lastPage) return;
            var slideInName = isLeft ? "SlideInLeft" : "SlideInRight";
            var slideOut = FindResource("SlideOut") as Storyboard;
            var slideIn = FindResource(slideInName) as Storyboard;
            if (lastPage != null)
                slideOut.Begin(lastPage, true);
            slideIn.Begin(content, true);
            lastPage = content;
        }
        public MainWindow()
        {
            Global.LoadSettings();
            Global.ListenToEvent("EnableMagnet", SetEnableMagnet);
            Global.ListenToEvent("ColorSkin", SetColorSkin);
            Global.ListenToEvent("Theme", SetTheme);
            InitializeComponent();
            Artwork.DataBus.DataBus.Set("MainWindow", this);
            DataContext = this;
            MessageBus.Instance.Subscribe(this);
            loadSongLists();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            ActionBarService.RegisterActionBar(this.charmBar);
            Artwork.DataBus.DataBus.Set("list_download", list_download);
            SizeChanged += MainWindow_SizeChanged;
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
                foreach (var item in lists)
                {
                    if (item.IsHitTestVisible) return item;
                }
                return lists[0];
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
            btn_plause.Content = Mp3Player.GetPlayOrPause(id);
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
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        void OnMp3PlayerSongChanged(object sender, SongChangedEventArgs e)
        {
            var now = SongViewModel.NowPlaying;
            part_nowPlaying.DataContext = now;
            Title = string.Format("{0} - {1}      ", now.Name, now.ArtistName);
            counter = 0;
            if (trayIcon != null)
            {
                if (Global.AppSettings["ShowNowPlaying"] == "0") return;
                balloonTip = new MyBalloonTip();
                balloonTip.ViewModel = now;
                trayIcon.ShowCustomBalloon(balloonTip, PopupAnimation.Fade, 3000);
            }
            refreshPlause();
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
            ActionBarService.RegisterContext("2", list_search, "SelectCount");
            ActionBarService.RegisterContext("3", list_download, "SelectCount");
            ActionBarService.RegisterContext("4", list_complete, "SelectCount");
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
            currentList.PerformAction("选中正在播放");
        }

        private void btn_plause_Click(object sender, RoutedEventArgs e)
        {
            currentList.PerformAction("播放/暂停");
            refreshPlause();
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            currentList.PerformAction("下一首");
        }
    }
}

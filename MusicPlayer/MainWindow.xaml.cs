using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Hardcodet.Wpf.TaskbarNotification;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using Jean_Doe.MusicControl;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
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
        static FileSystemWatcher watcher;
        public MainWindow()
        {
            Global.LoadSettings();
            Global.ListenToEvent("EnableMagnet", SetEnableMagnet);
            Global.ListenToEvent("ColorSkin", SetColorSkin);
            Global.ListenToEvent("Theme", SetTheme);
            Global.ListenToEvent("xiami_avatar", SetAvatar);
            InitializeComponent();
            Artwork.DataBus.DataBus.Set("MainWindow", this);
            DataContext = this;
            MessageBus.Instance.Subscribe(this);
            loadSongLists();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            new MusicSliderConnector(slider);
            ActionBarService.SetActionBar(this.charmBar);
            Artwork.DataBus.DataBus.Set("list_download", list_download);
            SizeChanged += MainWindow_SizeChanged;
            Mp3Player.SongChanged += OnMp3PlayerSongChanged;
            Global.ListenToEvent("TitleMarquee", SetTitleMarquee);
            RunProgramHelper.RunProgram("updater.exe", "check");
        }
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        void OnMp3PlayerSongChanged(object sender, SongChangedEventArgs e)
        {
            var now = list_complete.NowPlaying;
            Title = string.Format("{0} - {1}      ", now.Name, now.ArtistName);
            counter = 0;
            if (trayIcon != null)
            {

                if (Global.AppSettings["ShowNowPlaying"] == "0") return;
                balloonTip = new MyBalloonTip();
                balloonTip.ViewModel = now;
                trayIcon.ShowCustomBalloon(balloonTip, PopupAnimation.Slide, 3000);
            }
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
            ActionBarService.RegisterContext("1", userPage, "IsLoggedIn");
            ActionBarService.RegisterContext("2", list_search, "SelectCount");
            ActionBarService.RegisterContext("3", list_download, "SelectCount");
            ActionBarService.RegisterContext("4", list_complete, "SelectCount");
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
            catch (Exception ex)
            {
            }

            //tag control events
            foreach (var item in headers.Children)
            {
                var header = item as ToggleButton;
                if (header != null)
                    header.Click += (s, a) =>
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
        }

        private void loadSongLists()
        {
            list_complete.SavePath = "complete.xml";
            list_download.SavePath = "download.xml";
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
                if (list_complete.NowPlaying != null)
                    Title = string.Format("{0} - {1}      ", list_complete.NowPlaying.Name, list_complete.NowPlaying.ArtistName);
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
            }
        }
        void SetTheme(string s)
        {
            var colors = s.Split(" ".ToCharArray());
            App.Current.Resources["darkBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[0]) };
            App.Current.Resources["lightBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[1]) };
            App.Current.Resources["selectBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(colors[2]) };
        }
        void SetColorSkin(string s)
        {
            App.Current.Resources["skinBrush"] = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(s) };
        }
        void SetAvatar(string s)
        {
            if (!System.IO.File.Exists(s) || !XiamiClient.GetDefault().IsLoggedIn)
            {
                Avatar = null;
                return;
            }
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(s));
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            Avatar = bi;
        }
        BitmapSource avatar;
        public BitmapSource Avatar { get { return avatar; } set { avatar = value; Notify("Avatar"); } }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Mp3Player.Exit();
            list_download.Save();
            list_complete.Save();
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
    }
}

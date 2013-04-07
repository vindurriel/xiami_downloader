using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using Jean_Doe.MusicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
        IHandle<MsgChangeWindowState>,
        IHandle<MsgSetBusy>
    {
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
                int i = 1;
                foreach (var item in headers.Children)
                {
                    var header = item as ToggleButton;
                    header.IsChecked = i == page;
                    i += 1;
                }
                if (contents.Children.Count > page - 1)
                {
                    var content = contents.Children[page - 1] as FrameworkElement;
                    if (content != null)
                        showPage(content, isLeft);
                }
                ActionBarService.ContextName = page.ToString();
                ActionBarService.Refresh();
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
            Global.ListenToEvent("xiami_avatar", SetAvatar);
            InitializeComponent();
            DataContext = this;
            MessageBus.Instance.Subscribe(this);
            loadSongLists();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            new MusicSliderConnector(slider);
            slider.Visibility = Visibility.Collapsed;
            ActionBarService.SetActionBar(this.charmBar);
            Artwork.DataBus.DataBus.Set("list_download", list_download);
            Mp3Player.SongChanged += Mp3Player_SongChanged;
        }

        void Mp3Player_SongChanged(object sender, Mp3Player.SongChangedEventArgs e)
        {
            slider.Visibility = Visibility.Visible;
        }
        void initActionBar()
        {
            ActionBarService.RegisterContext("1", userPage, "IsLoggedIn");
            ActionBarService.RegisterContext("2", list_search, "SelectCount");
            ActionBarService.RegisterContext("3", list_download, "SelectCount");
            ActionBarService.RegisterContext("4", list_complete, "SelectCount");
            ActionBarService.RegisterContext("5", configPage, "IsDirty");
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initActionBar();
            //tag control events
            foreach (var item in headers.Children)
            {
                var header = item as ToggleButton;
                if (header != null)
                    header.Click += (s, a) =>
                    {
                        Page = headers.Children.IndexOf(s as UIElement) + 1;
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
        void SetColorSkin(string s)
        {
            try
            {
                App.Current.Resources["skinBrush"] = new SolidColorBrush
                {
                    Color = (Color)ColorConverter.ConvertFromString(s)
                };
            }
            catch { }
        }
        void SetAvatar(string s)
        {
            if (!System.IO.File.Exists(s))
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
                    UIHelper.RunOnUI(new Action(() =>
                    {
                        Page = 2;
                        busyIndicator.StartSpin();
                    }));
                    break;
                case EnumSearchState.Cancelling:
                case EnumSearchState.Finished:
                    UIHelper.RunOnUI(new Action(() =>
                    {
                        Page = 2;
                        busyIndicator.StopSpin();
                    }));
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
                    WindowState = WindowState.Minimized;
                    break;
                default:
                    break;
            }
        }

        public void Handle(MsgSetBusy message)
        {
            if (message.On)
                busyIndicator.StartSpin();
            else
                busyIndicator.StopSpin();
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}

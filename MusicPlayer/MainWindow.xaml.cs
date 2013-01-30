using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using Jean_Doe.MusicControl;
using System.Windows.Data;
namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged,
        IHandle<MsgSearchStateChanged>,
        IHandle<string>,
        IHandle<MsgChangeWindowState>,
        IHandle<MsgSetBusy>
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        internal class CharmBarEntity
        {
            private Binding b = new Binding();

            public Binding Binding
            {
                get { return b; }
                set { b = value; }
            }
            public List<CharmAction> Actions = new List<CharmAction>();
        }
        internal class CharmAction
        {
            public string Label { get; set; }
            public Action<object, RoutedEventArgs> Action { get; set; }
            public CharmAction(string l, Action<object, RoutedEventArgs> a)
            {
                Label = l;
                Action = a;
            }
        }
        readonly int pageCount = 4;
        FrameworkElement lastPage;
        List<CharmBarEntity> charms = new List<CharmBarEntity>();
        private int page;
        public int Page
        {
            get { return page; }
            set
            {
                if (value == page) return;
                bool isLeft = value > page;
                page = value;
                for (int i = 1; i < pageCount + 1; i++)
                {
                    var header = FindName("head" + i.ToString()) as ToggleButton;
                    header.IsChecked = i == page;
                }
                var content = FindName("page" + page.ToString()) as FrameworkElement;
                if (content != null)
                    showPage(content, isLeft);
                if (page == 0) return;
                charmBar.SetBinding(Expander.IsExpandedProperty, charms[page].Binding);
                charmBarItemWrapper.Children.Clear();
                var style=FindResource("charmBarButton") as Style;
                foreach (var item in charms[page].Actions)
	            {
                    var btn = new Button() { Content = item.Label };
                    btn.Click += new RoutedEventHandler(item.Action);
                    btn.Style = style;
                    charmBarItemWrapper.Children.Add(btn);
	            }      
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
            InitializeComponent();
            DataContext = this;
            MessageBus.Instance.Subscribe(this);
            loadSongLists();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            MouseLeftButtonDown += (s, e) => { DragMove(); };
            MouseDoubleClick += (s, e) =>
            {
                MessageBus.Instance.Publish(new MsgChangeWindowState { State = EnumChangeWindowState.Maximized });
            };
        }
        void initCharms()
        {
            var int2bool = FindResource("int2bool") as IntToBoolConverter;
            charms.Add(new CharmBarEntity());
            charms.Add(new CharmBarEntity
            {
                
                Binding = new Binding("SelectCount") { Converter = int2bool, Source = list_search },
                Actions = new List<CharmAction> 
                { 
                    new CharmAction("下载",this.btn_download_add_Click),
                },
            });
            charms.Add(new CharmBarEntity
            {
                Binding = new Binding("SelectCount") { Converter = int2bool, Source = list_download },
                Actions = new List<CharmAction> 
                { 
                    new CharmAction("开始",this.btn_download_start_Click),
                    new CharmAction("暂停",this.btn_cancel_Click),
                    new CharmAction("删除",this.btn_remove_Click),
                    new CharmAction("完成",this.btn_complete_Click),
                },
            });
            charms.Add(new CharmBarEntity
            {
                Binding = new Binding("SelectCount") { Converter = int2bool, Source = list_complete },
                Actions = new List<CharmAction> 
                { 
                    new CharmAction("存为播放列表",this.btn_save_playlist_Click),
                    new CharmAction("播放",this.btn_play_Click),
                    new CharmAction("删除",this.btn_remove_complete_Click),
                },
            });
            charms.Add(new CharmBarEntity
            {
                Binding = new Binding("IsDirty") { Source = configPage },
                Actions = new List<CharmAction> 
                { 
                    new CharmAction("保存",this.btn_save_config_Click),
                },
            });
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initCharms();
            //tag control events
            for (int i = 1; i < pageCount + 1; i++)
            {
                var header = FindName("head" + i.ToString()) as ToggleButton;
                if (header != null)
                    header.Click += (s, a) =>
                    {
                        Page = int.Parse((s as FrameworkElement).Name.Substring(4));
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
            //enable magnet 
            SetEnableMagnet(Global.AppSettings["EnableMagnet"]);
            //set skin color
            SetColorSkin(Global.AppSettings["ColorSkin"]);
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
                this.ListenToRimChanged(OnRimChanged);
            }
            else
            {
                this.DisableMagnet();
            }
        }
        void OnRimChanged(EnumRim rim)
        {
            var t = new Thickness(0);
            var width = 2;
            switch (rim)
            {
                case EnumRim.Top:
                    t.Bottom = width;
                    break;
                case EnumRim.Left:
                    t.Right = width;
                    break;
                case EnumRim.Right:
                    t.Left = width;
                    break;
                case EnumRim.Bottom:
                    t.Top = width;
                    break;
                default:
                    break;
            }
            border.BorderThickness = t;
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
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            list_download.Save();
            list_complete.Save();
            Global.AppSettings["ActivePage"] = Page.ToString();
            Global.AppSettings["WindowPos"] = new Rect(Left, Top, ActualWidth, ActualHeight).ToString();
            Global.SaveSettings();
        }
        void btn_download_add_Click(object sender, RoutedEventArgs e)
        {
            var list = list_search.SelectedItems.ToList();
            foreach (var item in list)
            {
                list_download.AddAndStart(item);
                list_search.Remove(item);
            }
            //Page = 2;
        }
        void btn_download_start_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StartByTag(list_download.SelectedItems.Select(x => x.Id).ToList());
        }

        void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StopByTag(list_download.SelectedItems.Select(x => x.Id).ToList());
        }
        void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.RemoveByTag(list_download.SelectedItems.Select(x => x.Id).ToList());
            var list = list_download.SelectedItems.ToList();
            foreach (var item in list)
            {
                list_download.Remove(item);
            }
        }
        void btn_remove_complete_Click(object sender, RoutedEventArgs e)
        {
            var list = list_complete.SelectedItems.ToList();
            foreach (var item in list)
            {
                list_complete.Remove(item);
            }
        }


        List<string> Playlist = new List<string>();
        int lastPageNum = 0;
        void btn_more_Click(object sender, RoutedEventArgs e)
        {

            if (more.IsChecked == true)
            {
                lastPageNum = Page;
                Page = 0;
            }
            else
            {
                Page = lastPageNum;
            }
            var anim = FindResource(more.IsChecked == true ? "anim_show_more" : "anim_hide_more") as Storyboard;
            anim.Begin();
        }
        void btn_play_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list_complete.SelectedItems)
            {
                if (!item.HasMp3) continue;
                if (!string.IsNullOrEmpty(item.Song.FilePath))
                {
                    Mp3Player.PlayPause(item.Song.FilePath);
                }
            }
        }
        void btn_save_config_Click(object sender, RoutedEventArgs e)
        {
            configPage.SaveConfig();
        }
        void btn_save_playlist_Click(object sender, RoutedEventArgs e)
        {
            var list = list_complete;
            if (!list.SelectedItems.Any())
                return;
            var win = new System.Windows.Forms.SaveFileDialog
            {
                InitialDirectory = Global.AppSettings["DownloadFolder"],
                Filter = "播放列表文件 (*.m3u)|*.m3u",
                OverwritePrompt = true,
                Title = "存为播放列表"
            };
            if (win.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SavePlaylist(win.FileName);
            }
        }
        void btn_complete_Click(object sender, RoutedEventArgs e)
        {
            var list = list_download;
            if (!list.SelectedItems.Any())
                return;
            DownloadManager.Instance.StopByTag(list_download.SelectedItems.Select(x => x.Id).ToList());
            foreach (var item in list.SelectedItems)
            {
                item.HasMp3 = true; item.HasLrc = true; item.HasArt = true;
                MessageBus.Instance.Publish(new MsgDownloadStateChanged
                {
                    Id = item.Id,
                    Item = item,
                });
            }
            Page = 3;
        }

        string SavePlaylist(string path = null)
        {
            var list = list_complete;
            if (!list.SelectedItems.Any())
                return null;
            Playlist.Clear();
            foreach (var item in list.SelectedItems)
            {
                if (!item.HasMp3) continue;
                Playlist.Add(string.Format("#EXTINF:{0}", item.Id));
                if (string.IsNullOrEmpty(item.Song.FilePath))
                    Playlist.Add(System.IO.Path.Combine(".", item.Dir, item.FileNameBase + ".mp3"));
                else
                    Playlist.Add(item.Song.FilePath);
            }
            if (path == null)
                path = Global.AppSettings["DownloadFolder"] + "\\default.m3u";
            File.WriteAllLines(path, Playlist, Encoding.UTF8);
            return path;
        }

        void SetStatus(string status)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                //statusBar.Text = status;
            }));
        }

        private void btn_config_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Handle(MsgSearchStateChanged message)
        {
            switch (message.State)
            {
                case EnumSearchState.Started:
                    UIHelper.RunOnUI(new Action(() =>
                    {
                        Page = 1;
                        busyIndicator.StartSpin();
                    }));
                    SetStatus("开始搜索");
                    break;
                case EnumSearchState.Working:
                    if (message.SearchResult == null) return;
                    SetStatus(string.Format("正在获取第{0}页", message.SearchResult.Page));
                    break;
                case EnumSearchState.Finished:
                    SetStatus("搜索完成");
                    UIHelper.RunOnUI(new Action(() =>
                    {
                        Page = 1;
                        busyIndicator.StopSpin();
                    }));
                    break;
                default:
                    break;
            }
        }
        public void Handle(string message)
        {
            SetStatus(message);
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
    }
}

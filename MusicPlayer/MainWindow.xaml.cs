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
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        //public enum EnumPage { one, two, three,four }

        readonly int pageCount = 4;
        FrameworkElement lastPage;
        private int page;
        public int Page
        {
            get { return page; }
            set
            {
                page = value;
                if(page == 1)
                {
                    head1.IsChecked = true;
                    expander1.IsExpanded = true;
                }
                else
                {
                    head1.IsChecked = false;
                    expander1.IsExpanded = false;
                }

                for(int i = 2; i < pageCount + 1; i++)
                {
                    var header = FindName("head" + i.ToString()) as ToggleButton;
                    header.IsChecked = i == page;
                }
                var content = FindName("page" + page.ToString()) as FrameworkElement;
                if(content != null)
                    showPage(content);
            }
        }
        void showPage(FrameworkElement content)
        {
            if(content == lastPage) return;
            var slideOut = FindResource("SlideOut") as Storyboard;
            var slideIn = FindResource("SlideIn") as Storyboard;
            if(lastPage != null)
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
        }

        ToggleButton head1;
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //tag control events
            head1 = expander1.Template.FindName("ExpanderButton", expander1) as ToggleButton;
            head1.Click += (s, a) => Page = 1;
            for(int i = 2; i < pageCount + 1; i++)
            {
                var header = FindName("head" + i.ToString()) as ToggleButton;
                if(header != null)
                    header.Click += (s, a) =>
                    {
                        Page = int.Parse((s as FrameworkElement).Name.Substring(4));
                    };
            }
            //load last window position
            var rect = Rect.Parse(Global.AppSettings["WindowPos"]);
            if(rect.Width * rect.Height != 0)
            {
                Left = rect.Left; Top = rect.Top; Width = rect.Width; Height = rect.Height;
            }
            if(Left < 0) Left = 0;
            if(Top < 0) Top = 0;
            //load last active page
			var lastPage = int.Parse(Global.AppSettings["ActivePage"]);
            Page = lastPage==1? 3 :lastPage;
            //enable magnet 
            SetEnableMagnet(Global.AppSettings["EnableMagnet"]);
            //set skin color
            SetColorSkin(Global.AppSettings["ColorSkin"]);
        }

        private void loadSongLists()
        {
            list_download.SavePath = "download.xml";
            list_download.Load();
            list_complete.SavePath = "complete.xml";
            list_complete.Load();
        }

        void SetEnableMagnet(string on)
        {
            if(on == "1")
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
            var t = new Thickness(1.5);
            var width = 2;
            switch(rim)
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
            foreach(var item in list)
            {
                list_download.AddAndStart(item);
                list_search.Remove(item);
            }
            Page = 2;
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
            foreach(var item in list)
            {
                list_download.Remove(item);
            }
        }
        void btn_remove_complete_Click(object sender, RoutedEventArgs e)
        {
            var list = list_complete.SelectedItems.ToList();
            foreach(var item in list)
            {
                list_complete.Remove(item);
            }
        }


        List<string> Playlist = new List<string>();

        void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var arg = SavePlaylist();
            RunProgramHelper.RunProgram(arg, "");
        }
        void btn_save_playlist_Click(object sender, RoutedEventArgs e)
        {
            var list = list_complete;
            if(!list.SelectedItems.Any())
                return;
            var win = new System.Windows.Forms.SaveFileDialog
            {
                InitialDirectory = Global.AppSettings["DownloadFolder"],
                Filter = "播放列表文件 (*.m3u)|*.m3u",
                OverwritePrompt = true,
                Title = "存为播放列表"
            };
            if(win.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SavePlaylist(win.FileName);
            }
        }
        void btn_complete_Click(object sender, RoutedEventArgs e)
        {
            var list = list_download;
            if(!list.SelectedItems.Any())
                return;
            DownloadManager.Instance.StopByTag(list_download.SelectedItems.Select(x => x.Id).ToList());
            foreach(var item in list.SelectedItems)
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
            if(!list.SelectedItems.Any())
                return null;
            Playlist.Clear();
            foreach(var item in list.SelectedItems)
            {
                if(!item.HasMp3) continue;
                Playlist.Add(string.Format("#EXTINF:{0}", item.Id));
                if(string.IsNullOrEmpty(item.Song.FilePath))
                    Playlist.Add(System.IO.Path.Combine(".", item.Dir, item.FileNameBase + ".mp3"));
                else
                    Playlist.Add(item.Song.FilePath);
            }
            if(path == null)
                path = Global.AppSettings["DownloadFolder"] + "\\default.m3u";
            File.WriteAllLines(path, Playlist, Encoding.UTF8);
            return path;
        }

        void SetStatus(string status)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                statusBar.Text = status;
            }));
        }

        private void btn_config_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Handle(MsgSearchStateChanged message)
        {
            switch(message.State)
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
                    if(message.SearchResult == null) return;
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
            switch(message.State)
            {
                case EnumChangeWindowState.Close:
                    Close();
                    break;
                case EnumChangeWindowState.Maximized:
                    var s = WindowState;
                    if(s == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                    }

                    else
                    {
                        SizeToContent = SizeToContent.Manual;
                        WindowState = WindowState.Maximized;
                    }

                    break;
                case EnumChangeWindowState.Minimized:
                    WindowState = WindowState.Minimized;
                    SizeToContent = SizeToContent.Manual;
                    break;
                default:
                    break;
            }
        }
        public void Handle(MsgSetBusy message)
        {
            if(message.On)
                busyIndicator.StartSpin();
            else
                busyIndicator.StopSpin();
        }
    }
}

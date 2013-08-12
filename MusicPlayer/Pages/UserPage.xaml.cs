using Awesomium.Core;
using Awesomium.Windows.Controls;
using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MusicPlayer
{
    /// <summary>
    /// UserPage.xaml 的交互逻辑
    /// </summary>
    public partial class UserPage : IActionProvider
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        public UserPage()
        {
            InitializeComponent();
            Global.ListenToEvent("xiami_avatar", SetIsLoggedIn);
            Global.ListenToEvent("xiami_nick_name", SetNickName);
            webcontrol = new WebControl();
            webcontrol.Width = 800;
            webcontrol.Height = 600;
            webcontrol.ShowJavascriptDialog += webcontrol_ShowJavascriptDialog;
            webcontrol.DocumentReady += (s, e) =>
            {
                webcontrol.ExecuteJavascript(
                     "document.documentElement.style.overflow = 'hidden';");
            };
            content.Content = webcontrol;
            webcontrol.Source = new Uri("http://localhost:8888");
            webcontrol.AddressChanged += webcontrol_AddressChanged;
            btn_save.Click += btn_save_Click;
        }

        void webcontrol_AddressChanged(object sender, UrlEventArgs e)
        {
            var re = new System.Text.RegularExpressions.Regex(@"/model/(\d+)");
            var m = re.Match(e.Url.ToString());
            if (m.Success)
            {
                username.Text = m.Groups[1].Value;
            }
        }

        void webcontrol_ShowJavascriptDialog(object sender, JavascriptDialogEventArgs e)
        {
            if (e.DialogFlags.HasFlag(JSDialogFlags.HasPromptField))
            {
                // It's a 'window.prompt'. You need to design your own
                // dialog for this.
            }
            else // Everything else can be presented with a MessageBox that can easily be designed using the available extensions.
            {
                e.Handled = true;
                e.Cancel = true;
                if (MessageBox.Show(e.Message,
                 String.Format("Awesomium.NET - {0}", e.FrameURL)) == MessageBoxResult.OK)
                {
                    e.Cancel = false;
                }
            }
        }
        WebControl webcontrol;
        void btn_save_Click(object sender, RoutedEventArgs e)
        {
            var bmp = webcontrol.Surface as WebViewPresenter;
            var png = new System.Windows.Media.Imaging.PngBitmapEncoder();
            png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)bmp.Image));
            png.Save(File.OpenWrite(Global.CWD("ok.png")));
        }
        public void SetNickName(string s)
        {
            if (string.IsNullOrEmpty(s))
                username.Text = "未登录，请先到设置页面登录虾米账户。";
            else
                username.Text = "欢迎，" + s;
        }
        public void SetIsLoggedIn(String s)
        {
            IsLoggedIn = !string.IsNullOrEmpty(s) && XiamiClient.GetDefault().IsLoggedIn;
        }
        private async void btn_user_song_Click(object sender, RoutedEventArgs e)
        {
            Jean_Doe.MusicControl.SongViewModel.ClearFav();
            await SearchManager.Search("user:lib", EnumSearchType.song);
        }
        private async void btn_user_artist_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:lib", EnumSearchType.artist);
        }
        private async void btn_user_album_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:lib", EnumSearchType.album);
        }
        private async void btn_user_collect_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:lib", EnumSearchType.collect);
        }
        private async void btn_user_daily_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:daily", EnumSearchType.song);
        }
        private async void btn_user_guess_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:guess", EnumSearchType.song);
        }
        private async void btn_collect_recommend_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:collect_recommend", EnumSearchType.collect);
        }
        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction>{
            new CharmAction("今日推荐歌单","\xE19F今日",btn_user_daily_Click,(s)=>true),
            new CharmAction("猜你喜欢","\xE19F猜",btn_user_guess_Click,(s)=>true),
            new CharmAction("推荐精选集","\xE19F精选集",btn_collect_recommend_Click,(s)=>true),
            new CharmAction("收藏的歌曲","\xE0A5歌曲",btn_user_song_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的艺术家","\xE0A5艺术家",btn_user_artist_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的专辑","\xE0A5专辑",btn_user_album_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的精选集","\xE0A5精选集",btn_user_collect_Click,(s)=>IsLoggedIn),
            };
        }
        private bool isLoggedIn;

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value; Notify("IsLoggedIn"); }
        }
    }

}

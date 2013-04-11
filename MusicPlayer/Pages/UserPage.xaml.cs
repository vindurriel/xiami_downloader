using Jean_Doe.Common;
using System.Collections.Generic;
using System.ComponentModel;
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
        }
        public void SetNickName(string s)
        {
            if (string.IsNullOrEmpty(s))
                username.Text = "未登录，请先到设置页面登录。";
            else
                username.Text = "欢迎，" + s;
        }
        public void SetIsLoggedIn(string s)
        {
            IsLoggedIn = !string.IsNullOrEmpty(s);
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

        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction>{
            new CharmAction("今日推荐歌单",btn_user_daily_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的歌曲",btn_user_song_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的艺术家",btn_user_artist_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的专辑",btn_user_album_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的精选集",btn_user_collect_Click,(s)=>IsLoggedIn),
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

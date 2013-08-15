using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// UserPage.xaml 的交互逻辑
    /// </summary>
    public partial class UserPage : IActionProvider
    {
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
            username.Text="欢迎，"+s;
        }
         public void  SetIsLoggedIn(string s)
        {
            isLoggedIn = !string.IsNullOrEmpty(s);
            ActionBarService.Refresh();
        }
        private async void btn_login_Click(object sender, RoutedEventArgs e)
        {
            await XiamiClient.GetDefault().Login();
        }
        private async void btn_user_song_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:me",EnumSearchType.song);
        }
        private async void btn_user_artist_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:me", EnumSearchType.artist);
        }
        private async void btn_user_album_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:me", EnumSearchType.album);
        }
        private async void btn_user_collect_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:me", EnumSearchType.collect);
        }
        private async void btn_user_daily_Click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search("user:daily", EnumSearchType.song);
        }

        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction>{
            new CharmAction("今日推荐歌单",btn_user_daily_Click,IsLoggedIn),
            new CharmAction("收藏的歌曲",btn_user_song_Click,IsLoggedIn),
            new CharmAction("收藏的艺术家",btn_user_artist_Click,IsLoggedIn),
            new CharmAction("收藏的专辑",btn_user_album_Click,IsLoggedIn),
            new CharmAction("收藏的精选集",btn_user_collect_Click,IsLoggedIn),
            };
        }
        bool isLoggedIn = false;
        bool IsLoggedIn(object s)
        {
            return isLoggedIn;
        }
    }

}

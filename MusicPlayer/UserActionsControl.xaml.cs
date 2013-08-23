using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// UserActionsControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserActionsControl : INotifyPropertyChanged
    {
        public UserActionsControl()
        {
            InitializeComponent();
            Global.ListenToEvent("xiami_avatar", SetAvatar);
            Global.ListenToEvent("xiami_avatar", SetIsLoggedIn);
            Global.ListenToEvent("xiami_nick_name", SetNickName);
            Source = new ObservableCollection<CharmAction>
            {
            new CharmAction("今日推荐歌单","\xE19F",btn_user_daily_Click,(s)=>true),
            new CharmAction("猜你喜欢","\xE19F",btn_user_guess_Click,(s)=>true),
            new CharmAction("推荐精选集","\xE19F",btn_collect_recommend_Click,(s)=>true),
            new CharmAction("收藏的歌曲","\xE0A5",btn_user_song_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的艺术家","\xE0A5",btn_user_artist_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的专辑","\xE0A5",btn_user_album_Click,(s)=>IsLoggedIn),
            new CharmAction("收藏的精选集","\xE0A5",btn_user_collect_Click,(s)=>IsLoggedIn),
            };
            menu_actions.ItemsSource = Source;
            this.MouseLeftButtonUp += UserActionsControl_MouseLeftButtonUp;
        }

        void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsOpen = false;
        }
        void UserActionsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsOpen = !IsOpen;
        }
        private bool isOpen;
        public bool IsOpen
        {
            get { return isOpen; }
            set { isOpen = value; Notify("IsOpen"); }
        }

        ObservableCollection<CharmAction> Source;
        BitmapSource avatar;
        public BitmapSource Avatar { get { return avatar; } set { avatar = value; Notify("Avatar"); } }
        void SetAvatar(string s)
        {
            if (!XiamiClient.GetDefault().IsLoggedIn)
            {
                Avatar = null;
                return;
            }
            ImageManager.Get(string.Format("user_{0}.jpg", Global.AppSettings["xiami_uid"]), s, (img) => Avatar = img);
        }
        public void SetNickName(string s)
        {
            //if (string.IsNullOrEmpty(s))
            //    username.Text = "未登录，请先到设置页面登录虾米账户。";
            //else
            //    username.Text = "欢迎，" + s;
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
        private bool isLoggedIn;

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value; }
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

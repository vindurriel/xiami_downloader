using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigPage : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        List<ConfigItem> Configs;
        private ObservableCollection<string> sourceGroup = new ObservableCollection<string>();

        public ObservableCollection<string> SourceGroup
        {
            get { return sourceGroup; }
        }
        Dictionary<string, List<ConfigItem>> groups = new Dictionary<string, List<ConfigItem>>();
        private ObservableCollection<ConfigItem> sourceItem = new ObservableCollection<ConfigItem>();

        public ObservableCollection<ConfigItem> SourceItem
        {
            get { return sourceItem; }
        }
        public ConfigPage()
        {
            InitializeComponent();
            Configs = new List<ConfigItem>() { 
                new LabelConfigItem("下载","DownloadFolder","下载位置"),
                new ButtonConfigItem("下载","浏览",btn_browse_Click),
                new ComboConfigItem("下载","FolderPattern","目录名称规则"),
                new ComboConfigItem("下载","SongnamePattern","歌曲名称规则"),
                new ComboConfigItem("下载","MaxConnection","同时下载数量"),
                new ComboConfigItem("外观","Theme","主题"),       
                new ColorConfigItem("外观","ColorSkin","特殊颜色"),
                new ToggleConfigItem("外观","ShowNowPlaying","显示正在播放悬浮窗"),
                new ToggleConfigItem("外观","ShowLyric","显示歌词"),       
                new ToggleConfigItem("外观","EnableMagnet","边界停靠"), 
                new ToggleConfigItem("外观","TitleMarquee","标题滚动显示"),
                new InputConfigItem("nest","url_nest","nest服务地址"),
                new ComboConfigItem("播放","PlayNextMode","下一首模式"),
                new InputConfigItem("虾米账户","xiami_username","用户名"),
                new PasswordConfigItem("虾米账户","xiami_password","密码"),
                new ButtonConfigItem("虾米账户","登录",btn_xiami_login_Click),
                new ButtonConfigItem("百度云账户","登录",btn_baidu_login_Click),
                new LabelConfigItem("百度云账户","baidu_access_token","令牌"),
                new LabelConfigItem("软件更新","UpdateInfo",""),
            };
            foreach (var item in Configs)
            {
                if (!groups.ContainsKey(item.GroupName))
                    groups[item.GroupName] = new List<ConfigItem>();
                groups[item.GroupName].Add(item);
            }
            foreach (var item in Configs.Select(x => x.GroupName).Distinct())
            {
                SourceGroup.Add(item);
            }
            list_groups.SelectionChanged += OnList_groupsSelectionChanged;
        }

        void OnList_groupsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SourceItem.Clear();
            var key = list_groups.SelectedItem.ToString();
            if (!groups.ContainsKey(key)) return;
            Task.Run(() =>
            {
                foreach (var item in groups[key])
                {
                    UIHelper.WaitOnUI(() => SourceItem.Add(item));
                }
            });
        }

        private async void btn_xiami_login_Click(object sender, RoutedEventArgs a)
        {
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            await XiamiClient.GetDefault().Login();
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, false));
        }
        private  void btn_baidu_login_Click(object sender, RoutedEventArgs a)
        {
            var b = new System.Windows.Controls.WebBrowser();
            b.Width = 800;
            b.Height = 600;
            b.Navigated += (s, e) =>
            {
                var m = new Regex("access_token=(.*?)&").Match(e.Uri.ToString());
                if (m.Success)
                {
                    Global.AppSettings["baidu_access_token"] = m.Groups[1].Value;
                    (((s) as WebBrowser).Parent as Window).Close();
                }
            };
            b.Navigate(new PCS_client().GetAccessTokenPage());
            var w = new Window();
            w.Content = b;
            w.ShowDialog();
        }

        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
                             {
                                 ShowNewFolderButton = true,
                                 RootFolder = Environment.SpecialFolder.MyComputer,
                                 SelectedPath = Global.AppSettings["DownloadFolder"],
                             };
            var res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                Global.AppSettings["DownloadFolder"] = dialog.SelectedPath;
            }
        }
    }
}

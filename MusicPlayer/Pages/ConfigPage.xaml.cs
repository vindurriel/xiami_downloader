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
namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigPage:INotifyPropertyChanged
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
                new ToggleConfigItem("外观","EnableMagnet","边界停靠"),
                new ColorConfigItem("外观","ColorSkin","主题颜色"),
                new ToggleConfigItem("外观","ShowNowPlaying","最小化时显示正在播放"),
                new ToggleConfigItem("外观","ShowLyric","显示歌词"),       
                new ComboConfigItem("播放","PlayNextMode","下一首模式"),
                new InputConfigItem("虾米账户","xiami_username","用户名"),
                new PasswordConfigItem("虾米账户","xiami_password","密码"),
                new ButtonConfigItem("虾米账户","登录",btn_xiami_login_Click),
            };
            foreach (var item in Configs.Select(x => x.GroupName).Distinct())
            {
                SourceGroup.Add(item);
            }
            list_groups.SelectionChanged += OnList_groupsSelectionChanged;
        }

        void OnList_groupsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SourceItem.Clear();
            foreach (var item in Configs.Where(x => x.GroupName == list_groups.SelectedItem.ToString()))
            {
                SourceItem.Add(item);
            }
        }

        private async void btn_xiami_login_Click(object sender, RoutedEventArgs e)
        {
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            await XiamiClient.GetDefault().Login();
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy(this, false));
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

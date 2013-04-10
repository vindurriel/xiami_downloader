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
    public enum EnumConfigControlType { label, combo, toggle, color, input, pass }
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigPage : IActionProvider
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
                new ButtonConfigItem("下载","","浏览",btn_browse_Click),
                new ComboConfigItem("下载","FolderPattern","目录名称规则"),
                new ComboConfigItem("下载","SongnamePattern","歌曲名称规则"),
                new ComboConfigItem("下载","MaxConnection","同时下载数量"),
                new ToggleConfigItem("外观","EnableMagnet","边界停靠"),
                new ColorConfigItem("外观","ColorSkin","主题颜色"),
                new ToggleConfigItem("外观","ShowNowPlaying","显示正在播放"),
                new ToggleConfigItem("外观","ShowLyric","显示歌词"),       
                new ComboConfigItem("播放","PlayNextMode","下一首模式"),
                new InputConfigItem("虾米账户","xiami_username","用户名"),
                new PasswordConfigItem("虾米账户","xiami_password","密码"),
                new ButtonConfigItem("虾米账户","","登录",btn_xiami_login_Click),
            };
            Loaded += (s, e) => LoadConfig();
            foreach (var item in Configs.Select(x => x.GroupName).Distinct())
            {
                SourceGroup.Add(item);
            }
            list_groups.SelectionChanged += OnList_groupsSelectionChanged;
            //pop_skin.Opened += pop_skin_Opened;
            //btn_xiami_login.Click += btn_xiami_login_Click;
            actions.Add(new CharmAction("保存", this.btn_save_config_Click, (s) => { return (s as ConfigPage).IsDirty; }));
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

        void pop_skin_Opened(object sender, EventArgs e)
        {
            //color_colorskin.SelectedColorChanged -= color_colorskin_SelectedColorChanged;
            //color_colorskin.SelectedColorChanged += color_colorskin_SelectedColorChanged;
        }

        void color_colorskin_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            IsDirty = e.NewValue.ToString() != Global.AppSettings["ColorSkin"];
        }
        Color colorSkin;

        void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsDirty = true;
        }
        void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsDirty = true;
        }

        void pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            IsDirty = true;
        }
        void btn_save_config_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }
        bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; Notify("IsDirty"); }
        }



        void LoadConfig()
        {
            foreach (var item in Configs)
            {
                item.Load();
            }
            IsDirty = false;
        }

        public void SaveConfig()
        {
            foreach (var item in Configs)
            {
                item.Save();
            }
            IsDirty = false;
        }
        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            var ui = Configs.FirstOrDefault(x => x.Key == "DownloadFolder").UI as TextBlock;
            var dialog = new System.Windows.Forms.FolderBrowserDialog
                             {
                                 ShowNewFolderButton = true,
                                 RootFolder = Environment.SpecialFolder.MyComputer,
                                 SelectedPath = ui.Text
                             };
            var res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                ui.Text = dialog.SelectedPath;
            }
        }
        private static void ComboSelect(ComboBox c, string s)
        {
            bool selected = false;
            foreach (ComboBoxItem item in c.Items)
            {
                if (item.Tag.ToString() == s)
                {
                    c.SelectedItem = item;
                    selected = true;
                    break;
                }
            }
            if (!selected && c.Items.Count > 0)
                c.SelectedIndex = 0;
        }
        void toggle_changed(object sender, RoutedEventArgs e)
        {
            IsDirty = true;
        }

        private void btn_skin_click(object sender, RoutedEventArgs e)
        {
            //if (pop_skin.IsOpen) return;
            //pop_skin.IsOpen = true;
            //colorSkin = color_colorskin.SelectedColor;
        }
        List<CharmAction> actions = new List<CharmAction>();

        public IEnumerable<CharmAction> ProvideActions()
        {
            return actions;
        }
        private string validateImage;

        public string ValidateImage
        {
            get { return validateImage; }
            set { validateImage = value; Notify("ValidateImage"); }
        }
    }
}

using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        static readonly Dictionary<string, EnumConfigControlType> map = new Dictionary<string, EnumConfigControlType>
         {
                {"DownloadFolder", EnumConfigControlType.label},
                {"FolderPattern", EnumConfigControlType.combo},
                {"SongnamePattern", EnumConfigControlType.combo},
                {"EnableMagnet", EnumConfigControlType.toggle},
                {"MaxConnection", EnumConfigControlType.combo},
                {"ColorSkin", EnumConfigControlType.color},
                {"ShowDetails", EnumConfigControlType.toggle},
                {"PlayNextMode", EnumConfigControlType.combo},
                {"xiami_username", EnumConfigControlType.input},
                {"xiami_password", EnumConfigControlType.pass},
                {"ShowNowPlaying", EnumConfigControlType.toggle},
            };
        public ConfigPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadConfig();
            pop_skin.Opened += pop_skin_Opened;
            btn_xiami_login.Click += btn_xiami_login_Click;
            actions.Add(new CharmAction("保存", this.btn_save_config_Click, (s) => { return (s as ConfigPage).IsDirty; }));
        }

        private async void btn_xiami_login_Click(object sender, RoutedEventArgs e)
        {
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy (this,true));
            await XiamiClient.GetDefault().Login();
            Artwork.MessageBus.MessageBus.Instance.Publish(new MsgSetBusy (this,false));
        }

        void pop_skin_Opened(object sender, EventArgs e)
        {
            color_colorskin.SelectedColorChanged -= color_colorskin_SelectedColorChanged;
            color_colorskin.SelectedColorChanged += color_colorskin_SelectedColorChanged;
        }

        void color_colorskin_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (color_colorskin.SelectedColor != colorSkin)
                IsDirty = true;
            colorSkin = color_colorskin.SelectedColor;
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
            foreach (var item in map)
            {
                var key = item.Key; var prefix = item.Value;
                if (!Global.AppSettings.ContainsKey(key)) continue;
                var value = Global.AppSettings[key];
                var x = FindName(prefix.ToString() + "_" + key.ToLower()) as FrameworkElement;
                if (x == null) continue;
                switch (prefix)
                {
                    case EnumConfigControlType.label:
                        var tb = x as TextBlock;
                        if (tb != null) tb.Text = value;
                        break;
                    case EnumConfigControlType.input:
                        var t = x as TextBox;
                        if (t != null) t.Text = value;
                        break;
                    case EnumConfigControlType.pass:
                        var p = x as PasswordBox;
                        if (p != null) p.Password = value;
                        break;
                    case EnumConfigControlType.combo:
                        ComboSelect(x as ComboBox, value);
                        break;
                    case EnumConfigControlType.toggle:
                        var tg = x as ToggleSwitch;
                        if (tg != null)
                            tg.IsOn = value == "1";
                        break;
                    case EnumConfigControlType.color:
                        var cp = x as ColorPicker.ColorPicker;
                        if (cp != null)
                            try
                            {
                                var c = (Color)ColorConverter.ConvertFromString(value);
                                cp.SelectedColor = c;
                            }
                            catch { }
                        break;
                    default:
                        break;
                }
            }
            IsDirty = false;
        }

        public void SaveConfig()
        {
            foreach (var item in map)
            {
                var key = item.Key; var prefix = item.Value;
                if (!Global.AppSettings.ContainsKey(key)) continue;
                string value = null;
                var x = FindName(prefix.ToString() + "_" + key.ToLower()) as FrameworkElement;
                if (x == null) continue;
                switch (prefix)
                {
                    case EnumConfigControlType.label:
                        var textBlock = x as TextBlock;
                        if (textBlock != null) value = textBlock.Text;
                        break;
                    case EnumConfigControlType.input:
                        var t = x as TextBox;
                        if (t != null) value = t.Text;
                        break;
                    case EnumConfigControlType.pass:
                        var p = x as PasswordBox;
                        if (p != null)
                        {
                            if (p.Password.Length != 32)
                                value = p.Password.ToMD5();
                            else
                                value = p.Password;
                        }
                        break;
                    case EnumConfigControlType.combo:
                        var comboBox = x as ComboBox;
                        if (comboBox != null)
                            value = (comboBox.SelectedItem as ComboBoxItem).Tag.ToString();
                        break;
                    case EnumConfigControlType.toggle:
                        var tg = x as ToggleSwitch;
                        if (tg != null)
                            value = tg.IsOn == true ? "1" : "0";
                        break;
                    case EnumConfigControlType.color:
                        var cp = x as ColorPicker.ColorPicker;
                        if (cp != null)
                            value = cp.SelectedColor.ToString();
                        break;
                    default:
                        break;
                }
                if (value != null && value != Global.AppSettings[key])
                {
                    Global.AppSettings[key] = value;
                }
            }
            IsDirty = false;
        }
        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
                             {
                                 ShowNewFolderButton = true,
                                 RootFolder = Environment.SpecialFolder.MyComputer,
                                 SelectedPath = label_downloadfolder.Text
                             };
            var res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                label_downloadfolder.Text = dialog.SelectedPath;
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
            if (pop_skin.IsOpen) return;
            pop_skin.IsOpen = true;
            colorSkin = color_colorskin.SelectedColor;
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
        private async void btn_login_Click(object sender, RoutedEventArgs e)
        {
            await XiamiClient.GetDefault().Login();

        }
    }
}

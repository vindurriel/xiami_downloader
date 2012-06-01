using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Jean_Doe.Common;
//using System.Windows.Forms;
namespace MusicPlayer
{
    public enum EnumConfigControlType { label, combo, toggle, color }
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigPage
    {
        static readonly Dictionary<string, EnumConfigControlType> map = new Dictionary<string, EnumConfigControlType>
         {
                {"DownloadFolder", EnumConfigControlType.label},
                {"FolderPattern", EnumConfigControlType.combo},
                {"SongnamePattern", EnumConfigControlType.combo},
                {"EnableMagnet", EnumConfigControlType.combo},
                {"MaxConnection", EnumConfigControlType.combo},
                {"ColorSkin", EnumConfigControlType.color},
            };
        public ConfigPage()
        {
            InitializeComponent();
            btn_save.Click += (s, e) => SaveConfig();
            Loaded += (s, e) => LoadConfig();
            pop_skin.LostFocus += pop_skin_LostFocus;
        }
        Color colorSkin;
        void pop_skin_LostFocus(object sender, RoutedEventArgs e)
        {
            if(color_colorskin.SelectedColor != colorSkin)
                IsDirty = true;
            colorSkin = color_colorskin.SelectedColor;
        }


        void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsDirty = true;
        }
        private bool isDirty;

        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                pop.Opacity = IsDirty ? 1 : 0;
            }
        }

        void LoadConfig()
        {
            foreach(var item in map)
            {
                var key = item.Key; var prefix = item.Value;
                if(!Global.AppSettings.ContainsKey(key)) continue;
                var value = Global.AppSettings[key];
                var x = FindName(prefix.ToString() + "_" + key.ToLower()) as FrameworkElement;
                if(x == null) continue;
                switch(prefix)
                {
                    case EnumConfigControlType.label:
                        var tb = x as TextBlock;
                        if(tb != null) tb.Text = value;
                        break;
                    case EnumConfigControlType.combo:
                        ComboSelect(x as ComboBox, value);
                        break;
                    case EnumConfigControlType.toggle:
                        var tg = x as CheckBox;
                        if(tg != null)
                            tg.IsChecked = value == "1";
                        break;
                    case EnumConfigControlType.color:
                        var cp = x as ColorPicker.ColorPicker;
                        if(cp != null)
                            try
                            {
                                cp.SelectedColor = (Color)ColorConverter.ConvertFromString(value);
                            }
                            catch { }
                        break;
                    default:
                        break;
                }
            }
            IsDirty = false;
        }

        void SaveConfig()
        {
            foreach(var item in map)
            {
                var key = item.Key; var prefix = item.Value;
                if(!Global.AppSettings.ContainsKey(key)) continue;
                string value = null;
                var x = FindName(prefix.ToString() + "_" + key.ToLower()) as FrameworkElement;
                if(x == null) continue;
                switch(prefix)
                {
                    case EnumConfigControlType.label:
                        var textBlock = x as TextBlock;
                        if(textBlock != null) value = textBlock.Text;
                        break;
                    case EnumConfigControlType.combo:
                        var comboBox = x as ComboBox;
                        if(comboBox != null)
                            value = (comboBox.SelectedItem as ComboBoxItem).Tag.ToString();
                        break;
                    case EnumConfigControlType.toggle:
                        var tg = x as CheckBox;
                        if(tg != null)
                            value = tg.IsChecked == true ? "1" : "0";
                        break;
                    case EnumConfigControlType.color:
                        var cp = x as ColorPicker.ColorPicker;
                        if(cp != null)
                            value = cp.SelectedColor.ToString();
                        break;
                    default:
                        break;
                }
                if(value != null && value != Global.AppSettings[key])
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
            if(res == System.Windows.Forms.DialogResult.OK)
            {
                label_downloadfolder.Text = dialog.SelectedPath;
            }
        }
        private static void ComboSelect(ComboBox c, string s)
        {
            bool selected = false;
            foreach(ComboBoxItem item in c.Items)
            {
                if(item.Tag.ToString() == s)
                {
                    c.SelectedItem = item;
                    selected = true;
                    break;
                }
            }
            if(!selected && c.Items.Count > 0)
                c.SelectedIndex = 0;
        }
        void toggle_changed(object sender, RoutedEventArgs e)
        {
            IsDirty = true;
        }

        private void btn_skin_click(object sender, RoutedEventArgs e)
        {
            if(pop_skin.IsOpen) return;
            pop_skin.IsOpen = true;
            colorSkin = color_colorskin.SelectedColor;
        }
    }
}

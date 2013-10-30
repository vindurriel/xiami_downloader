using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Jean_Doe.Common;
//using System.Windows.Forms;
namespace MusicPlayer
{
    public enum EnumConfigControlType { label, combo }
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow
    {
        static readonly Dictionary<string, EnumConfigControlType> map = new Dictionary<string, EnumConfigControlType>
        {
            {"DownloadFolder", EnumConfigControlType.label},
            {"FolderPattern", EnumConfigControlType.combo},
            {"SongnamePattern", EnumConfigControlType.combo},
        };
        public ConfigWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadConfig();
            Closing += (s, e) => SaveConfig();
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
                    case EnumConfigControlType.combo:
                        ComboSelect(x as ComboBox, value);
                        break;
                    default:
                        break;
                }
            }
            LoadMaxConnection();
        }

        void LoadMaxConnection()
        {
            ComboSelect(combo_maxconnection, Global.MaxConnection.ToString());
        }
        void SaveMaxConnection()
        {
            var sel = combo_maxconnection.SelectedItem as ComboBoxItem;
            if (sel != null)
                Global.MaxConnection = sel.Tag.ToString();
        }

        void SaveConfig()
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
                    case EnumConfigControlType.combo:
                        var comboBox = x as ComboBox;
                        if (comboBox != null)
                            value = (comboBox.SelectedItem as ComboBoxItem).Tag.ToString();
                        break;
                    default:
                        break;
                }
                if (value != null && value != Global.AppSettings[key])
                    Global.AppSettings[key] = value;
            }
            SaveMaxConnection();
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
                    return;
                }
            }
            if (!selected && c.Items.Count > 0)
                c.SelectedIndex = 0;
        }
    }
}

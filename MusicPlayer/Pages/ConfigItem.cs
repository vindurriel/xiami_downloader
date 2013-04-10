using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicPlayer
{

    public abstract class ConfigItem
    {
        public string GroupName { get; set; }
        public string Key { get; set; }
        public abstract string StrValue { get; set; }
        public string DisplayName { get; set; }
        public abstract FrameworkElement UI { get; }
        public void Save()
        {
            Global.AppSettings[Key] = StrValue;
        }
        public void Load()
        {
            StrValue = Global.AppSettings[Key];
        }
        public ConfigItem(string group, string key, string displayname)
        {
            GroupName = group;
            Key = key;
            DisplayName = displayname;
        }
    }
    public class LabelConfigItem : ConfigItem
    {
        public LabelConfigItem(string g, string k, string n) : base(g, k, n) { }
        TextBlock ui = new TextBlock();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.Text;
            }
            set
            {
                ui.Text = value;
            }
        }
    }
    public class InputConfigItem : ConfigItem
    {
        public InputConfigItem(string g, string k, string n) : base(g, k, n) { }
        TextBox ui = new TextBox();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.Text;
            }
            set
            {
                ui.Text = value;
            }
        }
    }
    public class PasswordConfigItem : ConfigItem
    {
        public PasswordConfigItem(string g, string k, string n) : base(g, k, n) { }
        PasswordBox ui = new PasswordBox();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.Password;
            }
            set
            {
                ui.Password = value;
            }
        }
    }
    public class ToggleConfigItem : ConfigItem
    {
        public ToggleConfigItem(string g, string k, string n) : base(g, k, n) { }
        ToggleSwitch ui = new ToggleSwitch();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.IsOn ? "1" : "0";
            }
            set
            {
                ui.IsOn = value == "1";
            }
        }
    }
    public class ComboConfigItem : ConfigItem
    {
        public ComboConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui.ItemsSource= Global.ValueOptions[Key];
            ui.DisplayMemberPath = "Key";
            ui.SelectedValuePath = "Value";
            
        }
        ComboBox ui = new ComboBox();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.SelectedValue.ToString();
            }
            set
            {
                ui.SelectedValue = value;
            }
        }
        private void comboSelect(ComboBox c, string s)
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
    }
    public class ColorConfigItem : ConfigItem
    {
        public ColorConfigItem(string g, string k, string n) : base(g, k, n) { }
        ColorPicker.ColorPicker ui = new ColorPicker.ColorPicker();
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        public override string StrValue
        {
            get
            {
                return ui.SelectedColor.ToString();
            }
            set
            {
                ui.SelectedColor = (Color)ColorConverter.ConvertFromString(value);
            }
        }
    }
}

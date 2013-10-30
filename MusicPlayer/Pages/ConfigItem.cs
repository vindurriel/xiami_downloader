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
        public virtual string StrValue { get; set; }
        public string DisplayName { get; set; }
        public abstract FrameworkElement UI { get; }
        protected virtual void Save()
        {
            Global.AppSettings[Key] = StrValue;
        }
        protected virtual void Load()
        {
            StrValue = Global.AppSettings[Key];
        }
        public ConfigItem(string group, string key, string displayname)
        {
            GroupName = group;
            Key = key;
            DisplayName = displayname;
        }
        protected void onUICreated()
        {
            Global.ListenToEvent(Key, LoadValue);
        }
        protected virtual void LoadValue(string value)
        {
            Load();
        }
    }
    public class LabelConfigItem : ConfigItem
    {
        public LabelConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui = new TextBlock();
            onUICreated();
        }
        TextBlock ui;
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
                UIHelper.RunOnUI(() => ui.Text = value);
            }
        }
    }
    public class InputConfigItem : ConfigItem
    {
        public InputConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui = new TextBox();
            onUICreated();
            ui.TextChanged += (s, e) => Save();
        }
        TextBox ui;
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
        public PasswordConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui = new PasswordBox();
            onUICreated();
            ui.PasswordChanged += (s, e) => Save();
        }
        PasswordBox ui;
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        protected override void Load()
        {
            if (Global.AppSettings[Key] != ui.Password)
                ui.Password = Global.AppSettings[Key];
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
        public ToggleConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui = new ToggleSwitch(); onUICreated();
            ui.IsOnChanged += (s, e) => Save();
        }
        ToggleSwitch ui;
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
            ui = new ComboBox();
            ui.ItemsSource = Global.ValueOptions[Key];
            ui.DisplayMemberPath = "Key";
            ui.SelectedValuePath = "Value";
            onUICreated();
            ui.SelectionChanged += (s, e) => Save();
        }
        ComboBox ui;
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
    }
    public class ColorConfigItem : ConfigItem
    {
        public ColorConfigItem(string g, string k, string n)
            : base(g, k, n)
        {
            ui = new ColorPicker.ColorPicker();
            onUICreated();
            ui.PreviewMouseLeftButtonUp += OnUiMouseLeftButtonUp;
        }

        void OnUiMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Save();
        }
        ColorPicker.ColorPicker ui;
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

    public class ButtonConfigItem : ConfigItem
    {
        public ButtonConfigItem(string g, string n, Action<object, RoutedEventArgs> clickEvent)
            : base(g, null, n)
        {
            this.DisplayName = null;
            ui = new Button { Content = n };
            ui.Click += (s, e) => { clickEvent(s, e); };
        }
        Button ui;
        public override FrameworkElement UI
        {
            get { return ui; }
        }
        protected override void Load()
        {
        }
        protected override void Save()
        {
        }
    }
}

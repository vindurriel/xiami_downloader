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

namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// ToggleSwitch.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {

        // Provide CLR accessors for the event
        public event RoutedEventHandler IsOnChanged
        {
            add { AddHandler(IsOnChangedEvent, value); }
            remove { RemoveHandler(IsOnChangedEvent, value); }
        }

        // Using a RoutedEvent
        public static readonly RoutedEvent IsOnChangedEvent = EventManager.RegisterRoutedEvent(
            "IsOnChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitch));


        public ToggleSwitch()
        {
            InitializeComponent();
            toggle.Click += toggle_Click;
        }
        void toggle_Click(object sender, RoutedEventArgs e)
        {
            IsOn = !IsOn;
        }
        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(false, IsOnChangedCallBack));
        static void IsOnChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as ToggleSwitch;
            var val = obj.IsOn;
            //obj.BgColor = val ? obj.OnColor : obj.OffColor;
            obj.RaiseEvent(new RoutedEventArgs(IsOnChangedEvent));
        }

        public Brush BgColor
        {
            get { return (Brush)GetValue(BgColorProperty); }
            set { SetValue(BgColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BgColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BgColorProperty =
            DependencyProperty.Register("BgColor", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White));


        public Brush SkinColor
        {
            get { return (Brush)GetValue(SkinColorProperty); }
            set { SetValue(SkinColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SkinColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SkinColorProperty =
            DependencyProperty.Register("SkinColor", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.Red));
    }

}

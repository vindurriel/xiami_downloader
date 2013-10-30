using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.MusicControl;
namespace Jean_Doe.MusicControl
{

    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class TitleBar
    {
        MessageBus Bus = MessageBus.Instance;
        public TitleBar()
        {
            InitializeComponent();
            this.MouseDoubleClick += new MouseButtonEventHandler(TouchTitleBar_MouseDoubleClick);
            Bus.Subscribe(this);
        }

        void TouchTitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Bus.Publish(new MsgChangeWindowState { State = EnumChangeWindowState.Maximized });
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TitleBar), new UIPropertyMetadata("TouchTitle"));
        

        void BtnMinimizeClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MsgChangeWindowState { State = EnumChangeWindowState.Minimized });
        }

        void BtnMaximizeClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MsgChangeWindowState { State = EnumChangeWindowState.Maximized });

        }
        void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MsgChangeWindowState { State = EnumChangeWindowState.Close });

        }


        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(TitleBar), new UIPropertyMetadata(null));

        

    }
}

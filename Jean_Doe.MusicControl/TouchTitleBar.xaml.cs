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
using Artwork.Messages;
namespace Artwork.Wpf.Controls
{

    /// <summary>
    /// Interaction logic for TouchTitleBar.xaml
    /// </summary>
    public partial class TouchTitleBar : UserControl, ICanChangeSkin
    {
        public event EventHandler EventBtnClose;
        public event EventHandler EventBtnMinimize;
        public event EventHandler EventBtnMaximize;
        public MessageBus.MessageBus Bus = MessageBus.MessageBus.Instance;
        public TouchTitleBar()
        {
            InitializeComponent();
            this.MouseDoubleClick += new MouseButtonEventHandler(TouchTitleBar_MouseDoubleClick);
            this.DataContext = this;
            MessageBus.MessageBus.Instance.Subscribe(this);
            this.Loaded += new RoutedEventHandler(TouchTitleBar_Loaded);
        }

        void TouchTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (ArtworkSkin.Current == null) return;
            this.Handle(new MessageSkinChanged(ArtworkSkin.Current));
        }

        void TouchTitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Bus.Publish(new MessageChangeWindowState(EnumWindows.TouchWindow, EnumChangeWindowStateCommand.Maximized));
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TouchTitleBar), new UIPropertyMetadata("TouchTitle"));

        void BtnSkinClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MessageCreateWindow("ConfigWindow"));
        }

        void BtnMinimizeClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MessageChangeWindowState(EnumWindows.TouchWindow, EnumChangeWindowStateCommand.Minimized));
        }

        void BtnMaximizeClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MessageChangeWindowState(EnumWindows.TouchWindow, EnumChangeWindowStateCommand.Maximized));
        }
        void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Bus.Publish(new MessageChangeWindowState(EnumWindows.TouchWindow, EnumChangeWindowStateCommand.Close));
        }

        public void Handle(MessageSkinChanged message)
        {
            try
            {
                this.SetColor("ColorSkin", message.Skin.ColorSkin);
            }
            catch (Exception)
            {
            }
        }
    }
}

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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar
    {
        public ProgressBar()
        {
            InitializeComponent();
            this.Loaded += ProgressBar_Loaded;
        }
        Storyboard story;
        void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            story = pb.Template.FindName("story", pb) as Storyboard;
            pb.IsIndeterminate = IsIndeterminate;
        }
        
        public void StopSpin()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                IsIndeterminate = false;
            }));
        }
        public void StartSpin()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                IsIndeterminate = true;
            }));
        }

        #region propdp

        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(ProgressBar), new UIPropertyMetadata(10.0));


        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set
            {
                SetValue(IsIndeterminateProperty, value);
                pb.IsIndeterminate = IsIndeterminate;
            }
        }

        // Using a DependencyProperty as the backing store for IsIndeterminate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(ProgressBar), new UIPropertyMetadata(false));


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressBar), new UIPropertyMetadata(0.0));




        #endregion

    }
}

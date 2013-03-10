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

namespace MusicPlayer
{
    /// <summary>
    /// ActionBar.xaml 的交互逻辑
    /// </summary>
    public partial class ActionBar
    {
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ActionBar), new PropertyMetadata(false));



        public double MaxLength
        {
            get { return (double)this.Resources["MaxLength"]; }
            set { this.Resources["MaxLength"] = value; }
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(double), typeof(ActionBar), new PropertyMetadata(150.0));



        public ActionBar()
        {
            InitializeComponent();
        }
        public System.Collections.IEnumerable ItemsSource
        {
            get { return charmBarItemWrapper.ItemsSource; }
            set { charmBarItemWrapper.ItemsSource = value; }
        }
        private void charmBarAct(object sender, RoutedEventArgs e)
        {
            var sel = charmBarItemWrapper.SelectedItem as CharmAction;
            if (sel != null && sel.IsActive)
                sel.Action(sel, null);
        }
        public void Validate(int selectCount)
        {
            foreach (var item in charmBarItemWrapper.Items.OfType<CharmAction>())
            {
                item.Validate(selectCount, item);
            }
        }
    }
}

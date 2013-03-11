using Jean_Doe.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MusicPlayer
{
    /// <summary>
    /// ActionBar.xaml 的交互逻辑
    /// </summary>
    public partial class ActionBar:IActionBar
    {
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
        public IEnumerable ItemsSource
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

        void IActionBar.ValidActions(IEnumerable<CharmAction> actions)
        {
            charmBarItemWrapper.ItemsSource = actions;
        }

        bool IActionBar.IsOpen
        {
            get
            {
                return IsExpanded;
            }
            set
            {
                IsExpanded = value;
            }
        }
    }
}

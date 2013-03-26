using Jean_Doe.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayer
{
    /// <summary>
    /// ActionBar.xaml 的交互逻辑
    /// </summary>
    public partial class ActionBar : IActionBar
    {
        public double MaxLength
        {
            get { return (double)this.Resources["MaxLength"]; }
            set { this.Resources["MaxLength"] = value; }
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(double), typeof(ActionBar), new PropertyMetadata(150.0));


        public int MaxItemCount
        {
            get { return (int)GetValue(MaxItemCountProperty); }
            set { SetValue(MaxItemCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxItemCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxItemCountProperty =
            DependencyProperty.Register("MaxItemCount", typeof(int), typeof(ActionBar), new PropertyMetadata(5));



        public ActionBar()
        {
            InitializeComponent();
            ListSource = new CollectionViewSource { Source = source}.View;
            ListSource.Filter = filter1;
            MenuSource = new CollectionViewSource { Source = source }.View;
            MenuSource.Filter = filter2;
            list.ItemsSource = ListSource;
            menu.ItemsSource = MenuSource;
        }
        bool filter1(object x)
        {
            var s=x as CharmAction;
            if(s==null) return false;
            var index=source.IndexOf(s);
            var name=s.Label;
            return index <= MaxItemCount;
        }
        bool filter2(object x)
        {
            var s = x as CharmAction;
            if (s == null) return false;
            var index = source.IndexOf(s);
            var name = s.Label;
            return index > MaxItemCount;
        }
        public ICollectionView ListSource { get; set; }
        public ICollectionView MenuSource { get; set; }
        private void charmBarAct(object sender, RoutedEventArgs e)
        {
            var sel = (sender as FrameworkElement).DataContext as CharmAction;
            if (sel != null)
                sel.Action(sel, null);
        }
        ObservableCollection<CharmAction> source = new ObservableCollection<CharmAction>();
        void IActionBar.ValidActions(IEnumerable<CharmAction> actions)
        {
            more_actions.IsOpen = false;
            source.Clear();
            var c = actions.Count();
            foreach (var item in actions.Take(MaxItemCount))
            {
                source.Add(item);
            }
            if (c > MaxItemCount)
            {
                source.Add(new CharmAction("\xE0C2", more_Click_1));
                foreach (var item in actions.Skip(MaxItemCount))
                {
                    source.Add(item);
                }
            }
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

        private void more_Click_1(object sender, RoutedEventArgs e)
        {
            more_actions.IsOpen = !more_actions.IsOpen;
        }
    }
}

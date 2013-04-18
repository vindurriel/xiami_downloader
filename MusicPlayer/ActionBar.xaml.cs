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
        public int MaxItemCount
        {
            get { return (int)GetValue(MaxItemCountProperty); }
            set { SetValue(MaxItemCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxItemCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxItemCountProperty =
            DependencyProperty.Register("MaxItemCount", typeof(int), typeof(ActionBar), new PropertyMetadata(7));

        public ActionBar()
        {
            InitializeComponent();
            open_more = new CharmAction("\xE0C2", more_Click_1);
            close_more = new CharmAction("\xE0C2", more_Click_2);
            list.ItemsSource = source1;
            menu.ItemsSource = source2;
            more_actions.LostFocus += (s, e) =>
            {
                more_actions.IsOpen = false;
            };
        }
        public ICollectionView ListSource { get; set; }
        public ICollectionView MenuSource { get; set; }
        private void charmBarAct(object sender, RoutedEventArgs e)
        {
            var sel = (sender as FrameworkElement).DataContext as CharmAction;
            if (sel == null) return;
            more_actions.IsOpen = false;
            sel.Action(sel, null);
            if (sel != open_more && sel != close_more)
                ActionBarService.Refresh();
        }
        ObservableCollection<CharmAction> source1 = new ObservableCollection<CharmAction>();
        ObservableCollection<CharmAction> source2 = new ObservableCollection<CharmAction>();
        void IActionBar.ValidActions(IEnumerable<CharmAction> actions)
        {
            more_actions.IsOpen = false;
            
            source1.Clear();
            source2.Clear();
            var c = actions.Count();
            foreach (var item in actions.Take(MaxItemCount))
            {
                source1.Add(item);
            }
            if (c > MaxItemCount)
            {
                source1.Add(open_more);
                foreach (var item in actions.Skip(MaxItemCount))
                {
                    source2.Add(item);
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
        CharmAction open_more;
        CharmAction close_more;
        private void more_Click_1(object sender, RoutedEventArgs e)
        {
            var ui=list.ItemContainerGenerator.ContainerFromItem(sender) as UIElement;
            more_actions.PlacementTarget = ui;
            more_actions.IsOpen = true;
            source1.Remove(open_more);
            source1.Add(close_more);
        }
        private void more_Click_2(object sender, RoutedEventArgs e)
        {
            more_actions.IsOpen = false;
            source1.Remove(close_more);
            source1.Add(open_more);
        }
    }
}

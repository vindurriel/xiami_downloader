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
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;

namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl, IHandle<MsgSearchStateChanged>
    {
        public SearchControl()
        {
            InitializeComponent();
            MessageBus.Instance.Subscribe(this);
            refresh();
            btn_search.DataContext = this;
            btn_search.Click += btn_search_Click;
            historySearch.SelectionChanged += historySearch_SelectionChanged;
            Loaded += SearchControl_Loaded;

        }

        void historySearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (historySearch.IsChanging) return;
            if (e.AddedItems.Count == 0) return;
            var key = (e.AddedItems[0] as HistorySearchItem).Key;
                btn_search_Click(this, null);
        }
        void SearchControl_Loaded(object sender, RoutedEventArgs e)
        {
            var source = new CollectionViewSource { Source = Enum.GetValues(typeof(EnumSearchType)).Cast<EnumSearchType>() }.View;
            source.Filter = searchTypeFilter;
            historySearch.TextChanged += (s, dd) =>
            {
                mask_filter.Visibility = Visibility.Collapsed;
            };
            historySearch.GotFocus += (s, dd) =>
            {
                mask_filter.Visibility = Visibility.Collapsed;
            };
            historySearch.LostFocus += (s, dd) =>
            {
                mask_filter.Visibility = string.IsNullOrEmpty(historySearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            };
        }
        static bool searchTypeFilter(object s)
        {
            var t = (EnumSearchType)s;
            bool res = false;
            switch (t)
            {
                case EnumSearchType.all:
                case EnumSearchType.song:
                case EnumSearchType.artist:
                case EnumSearchType.album:
                case EnumSearchType.collect:
                    res = true;
                    break;
                default:
                    break;
            }
            return res;
        }
        void refresh()
        {
            if (historySearch == null) return;
            historySearch.SavePath = SavePath;
            historySearch.Load();
        }
        public string SavePath { get { return System.IO.Path.Combine(Global.BasePath, "history_all.xml"); } }

        static T FindParentOfType<T>(DependencyObject e) where T : DependencyObject
        {
            var p = LogicalTreeHelper.GetParent(e);
            if (p == null) return null;
            if (p is T) return p as T;
            return FindParentOfType<T>(p);
        }

        async void btn_search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchManager.State == EnumSearchState.Finished)
                {
                    await SearchManager.Search(Key, SearchType);
                }
                else
                {
                    SearchManager.Cancel();
                }
            }
            catch (Exception ex)
            {
                Jean_Doe.Downloader.Logger.Error(ex);
            }
        }
        void btn_back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var i = historySearch.SelectedIndex;
                if (i < 0 || i > historySearch.Items.Count - 1) { CanGoBack = false; return; }
                historySearch.SelectedIndex = i + 1;
                //btn_search_Click(this, e);
            }
            catch (Exception ex)
            {
                Jean_Doe.Downloader.Logger.Error(ex);
            }
        }
        public void Handle(MsgSearchStateChanged message)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                switch (message.State)
                {
                    case EnumSearchState.Started:
                        CanGoBack = false;
                        btn_search.Content = "\xe1a4";
                        historySearch.Text = message.SearchResult.Keyword;
                        break;
                    case EnumSearchState.Working:
                        break;
                    default:
                        btn_search.Content = "\xe1a3";
                        CanGoBack = true;
                        break;
                }
            }));
        }
        public EnumSearchType SearchType
        {
            get
            {
                if (historySearch.SelectedItem == null) return EnumSearchType.song;
                return (historySearch.SelectedItem as HistorySearchItem).SearchType;
            }
        }
        public string Key
        {
            get
            {
                if (historySearch.SelectedItem == null) return null;
                return (historySearch.SelectedItem as HistorySearchItem).Key;
            }
        }


        public bool CanGoBack
        {
            get { return (bool)GetValue(CanGoBackProperty); }
            set { SetValue(CanGoBackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanGoBack.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanGoBackProperty =
            DependencyProperty.Register("CanGoBack", typeof(bool), typeof(SearchControl), new PropertyMetadata(false));


    }
}

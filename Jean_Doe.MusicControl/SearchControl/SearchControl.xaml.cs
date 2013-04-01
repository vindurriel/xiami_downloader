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
            hs.SelectionChanged += hs_SelectionChanged;

            Loaded += SearchControl_Loaded;

        }

        void hs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.combo_xiami_type.SelectedItem = hs.SelectedSearchType;
        }
        void SearchControl_Loaded(object sender, RoutedEventArgs e)
        {
            var source = new CollectionViewSource { Source = Enum.GetValues(typeof(EnumSearchType)).Cast<EnumSearchType>() }.View;
            source.Filter = searchTypeFilter;
            combo_xiami_type.ItemsSource = source;
            var xtype = EnumSearchType.all;
            combo_xiami_type.SelectedItem = xtype;
            combo_xiami_type.SelectionChanged += this.combo_xiami_type_SelectionChanged_1;
            hs.TextChanged += (s, dd) =>
            {
                mask_filter.Visibility = Visibility.Collapsed;
            };
            hs.GotFocus += (s, dd) =>
            {
                mask_filter.Visibility = Visibility.Collapsed;
            };
            hs.LostFocus += (s, dd) =>
            {
                mask_filter.Visibility = string.IsNullOrEmpty(hs.Text) ? Visibility.Visible : Visibility.Collapsed;
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
                case EnumSearchType.user_song:
                case EnumSearchType.user_artist:
                case EnumSearchType.user_album:
                case EnumSearchType.user_collect:
                    res = true;
                    break;
                default:
                    break;
            }
            return res;
        }
        void refresh()
        {
            if (hs == null) return;
            hs.SavePath = SavePath;
            hs.Load();
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
                    hs.Upsert(SearchType, Key, 0);
                    await SearchManager.Search(Key);
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
        public void Handle(MsgSearchStateChanged message)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                switch (message.State)
                {
                    case EnumSearchState.Started:
                        img_search.Visibility = Visibility.Collapsed;
                        img_stop.Visibility = Visibility.Visible;
                        break;
                    case EnumSearchState.Working:
                        break;
                    default:
                        img_stop.Visibility = Visibility.Collapsed;
                        img_search.Visibility = Visibility.Visible;
                        break;
                }
            }));
        }
        public EnumSearchType SearchType
        {
            get
            {
                if (combo_xiami_type.SelectedItem == null)
                    return EnumSearchType.song;
                return (EnumSearchType)combo_xiami_type.SelectedItem;
            }
        }
        public string Key
        {
            get
            {
                return hs.Text.Trim();
            }
        }
        private void combo_xiami_type_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var x = combo_xiami_type.SelectedItem;
            if (x == null) return;
            EnumSearchType t = EnumSearchType.song;
            if (!Enum.TryParse(x.ToString(), out t))
                return;
            if (t >= EnumSearchType.user_song)
            {
                hs.Text = "user:me";
                var s=t.ToString();
                Enum.TryParse(s.Substring("user_".Length) ,out t);
                combo_xiami_type.SelectedItem = t;
            }
            Global.AppSettings["SearchResultType"] = t.ToString();
        }
    }
}

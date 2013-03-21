using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Artwork.MessageBus;
using Jean_Doe.Common;
using System.Windows.Data;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for SongListControl.xaml
    /// </summary>
    public partial class SongListControl : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        #region list methods
        public virtual void Add(SongViewModel song)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                items.Add(song);
            }));
        }
        public virtual void Insert(int index, SongViewModel song)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                items.Insert(index, song);
            }));
        }
        public void Remove(SongViewModel song)
        {
            UIHelper.RunOnUI(() =>
            {
                var ui = listView.ItemContainerGenerator.ContainerFromItem(song);
                if (ui == null) return;
                var storyboard = this.FindResource("FadeOut") as Storyboard;
                Storyboard.SetTarget(storyboard, ui);
                storyboard.Completed += (s, e) =>
                    items.Remove(song);
                storyboard.Begin();
            });
        }
        public SongViewModel GetItemById(string id)
        {
            var res = items.FirstOrDefault(x => x.Id == id) as SongViewModel;
            return res;
        }
        public void Clear()
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                items.Clear();
            }));
        }
        #endregion
        public ListView ListView { get { return listView; } }
        MusicViewModelList items;
        public MusicViewModelList Items { get { return items; } }
        private int itemsCount;
        public int ItemsCount
        {
            get { return itemsCount; }
            set
            {
                itemsCount = value; Notify("ItemsCount");
            }
        }
        private int selectCount;
        public int SelectCount { get { return selectCount; } set { selectCount = value; Notify("SelectCount"); } }
        object nowPlaying = null;
        public object NowPlaying { get { return nowPlaying; } set { nowPlaying = value; Notify("NowPlaying"); } }
        public IEnumerable<SongViewModel> SelectedSongs
        {
            get
            {
                return listView.SelectedItems.OfType<SongViewModel>();
            }
            set
            {
                listView.SelectedItem = value.FirstOrDefault();
            }
        }
        public IEnumerable<MusicViewModel> SelectedItems
        {
            get
            {
                return listView.SelectedItems.OfType<MusicViewModel>();
            }
        }
        public SongListControl()
        {
            InitializeComponent();
            input_filter.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(input_filter.Text))
                    mask_filter.Visibility = Visibility.Collapsed;

            };
            input_filter.TextChanged += input_filter_TextChanged;
            input_filter.GotFocus += (s, e) =>
                {
                    mask_filter.Visibility = Visibility.Collapsed;
                };
            input_filter.LostFocus += (s, e) =>
            {
                mask_filter.Visibility = string.IsNullOrEmpty(input_filter.Text) ? Visibility.Visible : Visibility.Collapsed;
            };
            combo_sort.SelectionChanged += (s, e) => ApplySort();
            Global.ListenToEvent("ShowDetails", HandleShowDetails);
            MessageBus.Instance.Subscribe(this);
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            Source = CollectionViewSource.GetDefaultView(items);
            Source.Filter = filter;
            Source.CollectionChanged += Source_CollectionChanged;
            listView.DataContext = Source;
            listView.SelectionChanged += dataGrid_SelectionChanged;
            HandleShowDetails(Global.AppSettings["ShowDetails"]);

        }

        void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var i = 0;
            foreach (var item in (sender as ICollectionView))
            {
                i++;
            }
            emptyText.Visibility = i > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void input_filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter_text = input_filter.Text.ToLower();
            Source.Refresh();
            var i = 0;
            foreach (var item in Source)
            {
                i++;
            }
            btn_search.Visibility = (i == 0 && !string.IsNullOrEmpty(filter_text)) ? Visibility.Visible : Visibility.Collapsed;
        }
        public ICollectionView Source { get; set; }
        public void UnselectAll()
        {
            listView.UnselectAll();
        }
        void HandleShowDetails(string show)
        {
            if (show == "1")
            {
                //col_artist.Visibility = Visibility.Visible;
                //col_album.Visibility = Visibility.Visible;
            }
            else
            {
                //col_album.Visibility = Visibility.Collapsed;
                //col_artist.Visibility = Visibility.Collapsed;
            }
        }

        public string SavePath { get { return Items.SavePath; } set { Items.SavePath = value; } }
        public virtual void Save()
        {
            items.Save();
        }
        public virtual void Load()
        {
            Task.Run(async () => { await items.Load(); });
        }

        protected virtual void btn_open_click(object sender, RoutedEventArgs e)
        {
            SongViewModel s = null;
            try
            {
                s = (sender as FrameworkElement).DataContext as SongViewModel;
            }
            catch { }
            if (s == null || !s.CanOpen) return;
            e.Handled = true;
            s.Open();
        }

        protected virtual async void link_album(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasAlbum>().FirstOrDefault();
            if (t == null) return;
            var id = t.AlbumId;
            await SearchManager.GetMusic(t.AlbumId, EnumSearchType.album);
        }

        protected virtual async void link_artist(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.GetMusic(t.ArtistId, EnumSearchType.artist_song);
        }
        protected virtual async void link_similar_artist(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.GetMusic(t.ArtistId, EnumSearchType.artist_similar);
        }
        protected virtual async void link_artist_album(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.GetMusic(t.ArtistId, EnumSearchType.artist_album);
        }
        protected virtual async void link_collection(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasCollection>().FirstOrDefault();
            if (t == null) return;
            await SearchManager.GetMusic(t.CollectionId, EnumSearchType.collect);
        }

        protected virtual void go_song(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as MusicViewModel;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoSong(t.Id), null);
        }
        protected virtual void go_artist(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasArtist;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoArtist(t.ArtistId), null);
        }
        protected virtual void go_album(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasAlbum;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoAlbum(t.AlbumId), null);
        }
        protected virtual void go_collect(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as MusicViewModel;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoCollect(t.Id), null);
        }
        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                ItemsCount = Items.Count;
            }));
            Save();
        }
        void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIHelper.RunOnUI(new Action(() =>
          {
              SelectCount = listView.SelectedItems.Count;
          }));
        }

        private void select_all_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }
        private void unselect_all_Click(object sender, RoutedEventArgs e)
        {
            listView.UnselectAll();
        }
        private async void btn_search_click(object sender, RoutedEventArgs e)
        {
            await SearchManager.Search(filter_text);
        }
        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SongViewModel;
            if (item == null) return;
            PlayItem = item;
        }
        private static SongViewModel playItem = null;

        public static SongViewModel PlayItem
        {
            get { return playItem; }
            set
            {
                if (playItem == value)
                {
                    playItem.TogglePlay();
                    return;
                }
                if (playItem != null)
                    playItem.Stop();
                playItem = value;
                if (playItem != null)
                    playItem.Play();
            }
        }

        private void Image_SourceUpdated_1(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var img = sender as Image;
            //Show(img);
        }

        private static void Show(UIElement obj)
        {
            var da = new DoubleAnimation
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.HoldEnd,
                Duration = TimeSpan.FromMilliseconds(200),
            };
            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, new PropertyPath("(UIElement.Opacity)"));
            var sb = new Storyboard();
            sb.Children.Add(da);
            sb.Begin();
        }
        protected List<CharmAction> actions = new List<CharmAction>();
        protected void btn_cancel_selection_Click(object sender, RoutedEventArgs e)
        {
            UnselectAll();
        }
        protected bool defaultActionValidate(object s)
        {
            return (s as SongListControl).SelectedItems.Any();
        }
        protected bool IsType<TInterface>(object source) where TInterface : IHasMusicPart
        {
            var s = (source as SongListControl);
            return s.SelectedItems.Count(x => x is TInterface) == 1;
        }
        private void ApplySort()
        {
            var tag = (combo_sort.SelectedItem as ComboBoxItem).Tag.ToString();
            if (tag == "Default_Asc")
            {
                Source.SortDescriptions.Clear();
                return;
            }
            var prop = tag.Split("_".ToCharArray())[0];
            var order = tag.Split("_".ToCharArray())[1];
            using (Source.DeferRefresh())
            {
                Source.SortDescriptions.Clear();
                Source.SortDescriptions.Add(new SortDescription(prop,
                    order == "Asc" ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }
        }
        string filter_text = "";
        bool filter(object sender)
        {
            if (string.IsNullOrWhiteSpace(input_filter.Text))
            {
                return true;
            };
            var music = sender as MusicViewModel;
            if (music == null)
            {
                return false;
            };
            return music.SearchStr.Contains(filter_text);
        }
    }
}

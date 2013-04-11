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
using System.Windows.Input;
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
        #region drag drop
        Point startPoint;
        List<SongViewModel> draggingSongs = new List<SongViewModel>();
        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(listView);
            draggingSongs.Clear();
            draggingSongs.AddRange(SelectedSongs);
        }
        private void List_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(listView);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > 10 || Math.Abs(diff.Y) > 10))
            {
                var files = new System.Collections.Specialized.StringCollection();
                foreach (var item in draggingSongs)
                {
                    if (string.IsNullOrEmpty(item.Song.FilePath))
                        files.Add(System.IO.Path.Combine(".", item.Dir, item.FileNameBase + ".mp3"));
                    else
                        files.Add(item.Song.FilePath);
                }
                if (files.Count == 0) return;
                var source = (e.OriginalSource as FrameworkElement).DataContext as SongViewModel;
                if (source == null) return;
                var dragData = new DataObject();
                dragData.SetFileDropList(files);
                DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Copy);

            }
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
        SongViewModel nowPlaying = null;
        public SongViewModel NowPlaying { get { return nowPlaying; } set { nowPlaying = value; Notify("NowPlaying"); } }
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
            MessageBus.Instance.Subscribe(this);
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            Source = CollectionViewSource.GetDefaultView(items);
            Source.Filter = filter;
            Source.CollectionChanged += Source_CollectionChanged;
            listView.DataContext = Source;
            listView.SelectionChanged += dataGrid_SelectionChanged;
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
            var s = SelectedSongs.FirstOrDefault();
            if (s == null || !s.CanOpen) return;
            s.Open();
        }

        protected virtual async void link_album(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasAlbum>().FirstOrDefault();
            if (t == null) return;
            var id = t.AlbumId;
            await SearchManager.Search("album:" + id, EnumSearchType.song);
            //await SearchManager.GetMusic(t.AlbumId, EnumSearchType.album);
        }

        protected virtual async void link_artist(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.Search("artist:" + t.ArtistId, EnumSearchType.song);
            //await SearchManager.GetMusic(t.ArtistId, EnumSearchType.artist_song);
        }
        protected virtual async void link_similar_artist(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.Search("artist:" + t.ArtistId, EnumSearchType.artist);
            //await SearchManager.GetMusic(t.ArtistId, EnumSearchType.artist_similar);
        }
        protected virtual async void link_artist_album(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            await SearchManager.Search("artist:" + t.ArtistId, EnumSearchType.album);
        }
        protected virtual async void link_collection(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasCollection>().FirstOrDefault();
            if (t == null) return;
            await SearchManager.Search("collect:" + t.CollectionId, EnumSearchType.song);
        }

        protected virtual void go_song(object sender, RoutedEventArgs e)
        {
            var t = sender as MusicViewModel;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoSong(t.Id), null);
        }
        protected virtual void go_artist(object sender, RoutedEventArgs e)
        {
            var t = sender as IHasArtist;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoArtist(t.ArtistId), null);
        }
        protected virtual void go_album(object sender, RoutedEventArgs e)
        {
            var t = sender as IHasAlbum;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoAlbum(t.AlbumId), null);
        }
        protected virtual void go_collect(object sender, RoutedEventArgs e)
        {
            var t = sender as MusicViewModel;
            if (t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoCollect(t.Id), null);
        }

        protected void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            var music = SelectedItems.FirstOrDefault();
            if (music == null) return;
            if (music is SongViewModel)
            {
                go_song(music, null);
            }
            else if (music is AlbumViewModel)
            {
                go_album(music, null);
            }
            else if (music is ArtistViewModel)
            {
                go_artist(music, null);
            }
            else if (music is CollectViewModel)
            {
                go_collect(music, null);
            }
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

        private void Image_SourceUpdated_1(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var img = sender as Image;
            //Show(img);
        }
        protected virtual void btn_play_Click(object sender, RoutedEventArgs e)
        {
        }

        protected void btn_fav_Click(object sender, RoutedEventArgs e)
        {
            var tasks = new List<Task>();
            foreach (var item in SelectedSongs)
            {
                item.InFav = true;
                Task.Run(async () =>
                {
                    await XiamiClient.GetDefault().Fav_Song(item.Id);
                });
            }
        }
        protected void btn_unfav_Click(object sender, RoutedEventArgs e)
        {
            var tasks = new List<Task>();
            foreach (var item in SelectedSongs)
            {
                item.InFav = false;
                Task.Run(async () =>
              {
                  await XiamiClient.GetDefault().Unfav_Song(item.Id);
              });
            }
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
        protected bool IsOnlyType<TInterface>(object source) where TInterface : IHasMusicPart
        {
            var s = (source as SongListControl);
            return s.SelectedItems.Count(x => x is TInterface) == 1;
        }
        protected bool IsType<TInterface>(object source) where TInterface : IHasMusicPart
        {
            var s = (source as SongListControl);
            return s.SelectedItems.Count() > 0 && s.SelectedItems.All(x => x is TInterface);
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

        private void item_double_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btn_play_Click(sender, e);
            ActionBarService.Refresh();
        }

        private void btn_more_Click_1(object sender, RoutedEventArgs e)
        {
        }
        protected static bool canFav(object o)
        {
            var s = (o as SongListControl).SelectedSongs;
            return s.Count() > 0 && s.Any(x => !x.InFav);
        }
        protected static bool canUnfav(object o)
        {
            var s = (o as SongListControl).SelectedSongs;
            return s.Count() > 0 && s.Any(x => x.InFav);
        }
    }
}

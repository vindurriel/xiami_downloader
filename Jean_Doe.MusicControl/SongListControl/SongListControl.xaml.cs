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
using System.Windows.Media;
using System.Linq.Expressions;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for SongListControl.xaml
    /// </summary>
    public partial class SongListControl : INotifyPropertyChanged
    {
        public SongListControl()
        {
            InitializeComponent();
            Global.ListenToEvent("Theme", OnTheme);
            initInputFilter();
            initTimer();
            combo_sort.SelectionChanged += (s, e) => ApplySort();
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            Source = new ListCollectionView(items);
            virtualView.DataContext = Source;
            wrapView.DataContext = items;
            virtualView.SelectionChanged += OnSelectionChanged;
            wrapView.SelectionChanged += OnSelectionChanged;
            views = new List<ListView> { virtualView, wrapView };
            foreach (var v in views)
            {
                v.SelectionChanged += OnListViewSelectionChanged;
            }
            listView = IsDefaultList ? virtualView : wrapView;
            Loaded += SongListControl_Loaded;

        }
        double maxRec = 1;
        public double MaxRec { get { return maxRec; } set { maxRec = value; Notify("MaxRec"); } }
        void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var curView = sender as ListView;
            foreach (var v in views)
            {
                if (v == curView) continue;
                v.SelectedItem = curView.SelectedItem;
            }
        }
        List<ListView> views;
        void SongListControl_Loaded(object sender, RoutedEventArgs e)
        {
            btn_more.IsOn = IsDefaultList;
        }
        protected ListView listView;
        private void initTimer()
        {
            timer.Tick += OnTimerTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        bool isDirty;
        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ItemsCount = listView.Items.Count;
            isDirty = true;
            if (e.NewItems != null)
                foreach (var item in e.NewItems.OfType<MusicViewModel>())
                {
                    if (item.Recommends > MaxRec)
                        MaxRec = item.Recommends;
                }
        }

        private void initInputFilter()
        {
            input_filter.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(input_filter.Text))
                    mask_filter.Visibility = Visibility.Collapsed;
                lastFilter = input_filter.Text;
                Task.Run(() =>
                {
                    var thisFilter = lastFilter;
                    System.Threading.Thread.Sleep(1000);
                    if (thisFilter != lastFilter) return;
                    UIHelper.RunOnUI(() =>
                    {
                        ApplyFilter();
                    });
                });
            };
            input_filter.GotFocus += (s, e) =>
            {
                mask_filter.Visibility = Visibility.Collapsed;
            };
            input_filter.LostFocus += (s, e) =>
            {
                mask_filter.Visibility = string.IsNullOrEmpty(input_filter.Text) ? Visibility.Visible : Visibility.Collapsed;
            };
        }
        string lastFilter = "";
        void OnTimerTick(object sender, EventArgs e)
        {

            Save();
        }

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
        public virtual void Move(int old, int newi)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                items.Move(old, newi);
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
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        public void Clear()
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                var s = FindVisualChild<ScrollViewer>(listView);
                if (s != null)
                    s.ScrollToTop();
                items.Clear();
                MaxRec = 1;
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
        private string theme;

        public string ThemeColor
        {
            get { return theme; }
            set { theme = value; Notify("ThemeColor"); }
        }
        public ListView ListView { get { return listView; } }
        MusicViewModelList items;
        public MusicViewModelList Items { get { return items; } }
        protected MusicViewModelList playList = new MusicViewModelList();
        private int itemsCount;
        public int ItemsCount
        {
            get { return itemsCount; }
            set
            {
                itemsCount = value;
                emptyText.Visibility = value > 0 ? Visibility.Collapsed : Visibility.Visible;
                btn_search.Visibility = (value == 0 && !string.IsNullOrEmpty(filter_text)) ? Visibility.Visible : Visibility.Collapsed;
                Notify("ItemsCount");
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
        public void OnTheme(string s)
        {
            ThemeColor = ImageHelper.RefreshDefaultColor();
        }

        protected virtual void ApplyFilter()
        {
            filter_text = input_filter.Text.ToLower();
            Source.Filter = filter;
            ItemsCount = listView.Items.Count;
        }
        bool filter(object sender)
        {
            var music = sender as MusicViewModel;
            if (music == null)
            {
                return false;
            };
            return music.SearchStr.Contains(filter_text);
        }
        bool filter_select(object sender)
        {
            var music = sender as MusicViewModel;
            if (music == null)
            {
                return false;
            };
            return SelectedSongs.Contains(music);
        }

        public ListCollectionView Source { get; set; }
        public void UnselectAll()
        {
            listView.UnselectAll();
        }

        public string SavePath { get { return Items.SavePath; } set { Items.SavePath = value; } }
        public virtual void Save()
        {
            if (isDirty)
            {
                items.Save();
                isDirty = false;
            }
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
            if (t is AlbumViewModel)
                await SearchManager.Search("album:" + id, EnumSearchType.song);
            else
                await SearchManager.Search("album:" + id, EnumSearchType.all);
            //await SearchManager.GetMusic(t.AlbumId, EnumSearchType.album);
        }

        protected virtual async void link_artist(object sender, RoutedEventArgs e)
        {
            var t = listView.SelectedItems.OfType<IHasArtist>().FirstOrDefault();

            if (t == null) return;
            if (t is ArtistViewModel)
                await SearchManager.Search("artist:" + t.ArtistId, EnumSearchType.song);
            else
                await SearchManager.Search("artist:" + t.ArtistId, EnumSearchType.all);
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
            var t = listView.SelectedItems.OfType<CollectViewModel>().FirstOrDefault();
            if (t == null) return;
            await SearchManager.Search("collect:" + t.Id, EnumSearchType.song);
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
        DispatcherTimer timer = new DispatcherTimer();


        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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
        protected bool IsOnlyType<TInterface>(object source) where TInterface : class
        {
            var s = (source as SongListControl);
            return s.SelectedItems.Count(x => x is TInterface) == 1;
        }
        protected bool IsType<TInterface>(object source) where TInterface : class
        {
            var s = (source as SongListControl);
            return s.SelectedItems.Count() > 0 && s.SelectedItems.All(x => x is TInterface);
        }
        class Sorter : System.Collections.IComparer
        {
            private string propName;
            public string PropertyName
            {
                get { return propName; }
                set
                {
                    propName = value; info = typeof(SongViewModel).GetProperty(PropertyName);
                }
            }
            public bool IsAsc { get; set; }
            System.Reflection.PropertyInfo info;
            public int Compare(object x, object y)
            {
                var vx = info.GetValue(x) as IComparable;
                var vy = info.GetValue(y) as IComparable;
                var res = vx.CompareTo(vy);
                return IsAsc ? res : -res;
            }
        }
        protected virtual void ApplySort()
        {
            var tag = (combo_sort.SelectedItem as ComboBoxItem).Tag.ToString();
            if (tag == "Default_Asc")
            {
                Source.CustomSort = null;
                return;
            }
            var prop = tag.Split("_".ToCharArray())[0];
            var order = tag.Split("_".ToCharArray())[1];
            Source.SortDescriptions.Clear();
            Source.SortDescriptions.Add(new SortDescription(prop, order == "Asc" ? ListSortDirection.Ascending : ListSortDirection.Descending));
            ItemsCount = listView.Items.Count;
        }
        string filter_text = "";

        protected virtual void item_double_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var s = SelectedItems.FirstOrDefault();
            if (s == null) return;
            if (s is AlbumViewModel)
                link_album(sender, null);
            else if (s is CollectViewModel)
                link_collection(sender, null);
            else if (s is ArtistViewModel)
                link_artist(sender, null);
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
        protected T GetParentOf<T>(DependencyObject o, string name = null) where T : FrameworkElement
        {
            if (o == null) return null;
            var p = System.Windows.Media.VisualTreeHelper.GetParent(o);
            if (p == null) return null;
            if (p is T)
            {
                if (string.IsNullOrEmpty(name) || (p as T).Name == name)
                    return p as T;
            }
            return GetParentOf<T>(p, name);
        }
        private void toggle_detail(object sender, MouseEventArgs e)
        {
            var music = (sender as FrameworkElement).DataContext as MusicViewModel;
            if (!music.CanAnimate) return;
            music.CanAnimate = false;
            music.IsDetailShown = !music.IsDetailShown;
            var root = GetParentOf<Grid>(sender as FrameworkElement, "root");
            var main = root.FindName("main") as FrameworkElement;
            var detail = root.FindName("detail") as FrameworkElement;
            var to = music.IsDetailShown ? 0 : 60;
            var to2 = !music.IsDetailShown ? 5 : 60;
            var sb = new Storyboard();
            var da = new DoubleAnimation(to, new Duration(TimeSpan.FromSeconds(.5)));
            var da2 = new DoubleAnimation(to2, new Duration(TimeSpan.FromSeconds(.5)));
            detail.Height = 5;
            var ease = new QuinticEase { EasingMode = EasingMode.EaseOut };
            da.EasingFunction = da2.EasingFunction = ease;
            Storyboard.SetTarget(da, main);
            Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
            Storyboard.SetTarget(da2, detail);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Height"));
            sb.Children.Add(da);
            sb.Children.Add(da2);
            sb.Completed += (d, ef) => music.CanAnimate = true;
            sb.Begin();
        }
        private void btn_more_Click(object sender, RoutedEventArgs e)
        {
            bool more_on = btn_more.IsOn;
            btn_more.ToolTip = more_on ? "切换到格子" : "切换到列表";
            listView = more_on ? virtualView : wrapView;
            ItemsCount = listView.Items.Count;
            virtualPart.Visibility = more_on ? Visibility.Visible : Visibility.Collapsed;
            wrapView.Visibility = !more_on ? Visibility.Visible : Visibility.Collapsed;
            ActionBarService.Refresh();
        }

        public bool IsDefaultList
        {
            get { return (bool)GetValue(IsDefaultListProperty); }
            set { SetValue(IsDefaultListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDefaultList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDefaultListProperty =
            DependencyProperty.Register("IsDefaultList", typeof(bool), typeof(SongListControl), new PropertyMetadata(false));
    }
}

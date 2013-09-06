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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for SongListControl.xaml
    /// </summary>
    public partial class SongListControl : INotifyPropertyChanged, IActionProvider
    {
        public SongListControl()
        {
            InitializeComponent();
            Global.ListenToEvent("Theme", OnTheme);
            initInputFilter();
            combo_sort.SelectionChanged += (s, e) => ApplySort();
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            Source = new ListCollectionView(items);
            virtualView.DataContext = Source;
            virtualView.SelectionChanged += OnSelectionChanged;
            listView = virtualView;
            Global.ListenToEvent("PlayNextMode", OnPlayNextMode);
            this.PropertyChanged += OnPropertyChanged;
            btn_more.Click += btn_more_Click;
        }

        void btn_more_Click(object sender, RoutedEventArgs e)
        {
            if (btn_more.IsChecked == true)
            {
                Grid.SetRowSpan(virtualView, 1);
            }
            else {
                Grid.SetRowSpan(virtualView, 2);
            }
        }
        protected void addCommonActions()
        {
            var l = new List<CharmAction>
            {
                new CharmAction("取消选择","\xE10E",this.btn_cancel_selection_Click,isMultiSelect),
                new CharmAction("播放/暂停","\xE102",this.btn_play_Click,(s)=>{
                    string id=null;
                    var song=SelectedSongs.FirstOrDefault();
                    if (song != null)
                    id = song.Id;
                    this.actions["播放/暂停"].Icon = Mp3Player.GetPlayOrPause(id);
                    return true;
                }),
                new CharmAction("选中正在播放","\xE18B",this.btn_select_nowplaying_Click,(s)=>false),  
                new CharmAction("下一首","\xE101",this.btn_next_Click,(s)=>false),    
                new CharmAction("收藏","\xE0A5",this.btn_fav_Click,canFav),
                new CharmAction("不再收藏","\xE007",this.btn_unfav_Click,canUnfav),
                new CharmAction("查看专辑","\xE1d2",link_album,IsOnlyType<IHasAlbum>),
                new CharmAction("查看艺术家","\xe13d",link_artist,IsOnlyType<IHasArtist>),
                new CharmAction("查看专辑的歌曲","\xE189",link_album,IsOnlyType<AlbumViewModel>),
               new CharmAction("查看专辑的艺术家","\xe13d",link_artist,IsOnlyType<AlbumViewModel>),
               new CharmAction("查看精选集的歌曲","\xE189",link_collection,IsOnlyType<CollectViewModel>),
               new CharmAction("查看艺术家的最受欢迎歌曲","\xE189",link_artist,IsOnlyType<ArtistViewModel>),
               new CharmAction("查看艺术家的专辑","\xE1d2",link_artist_album,IsOnlyType<ArtistViewModel>),
               new CharmAction("查看艺术家的相似艺人","\xE125",link_similar_artist,IsOnlyType<ArtistViewModel>),
                new CharmAction("在浏览器中打开","\xE12B",this.btn_browse_Click,IsOnlyType<MusicViewModel>),
            };
            foreach (var item in l)
            {
                actions[item.Label] = item;
            }
        }
        double maxRec = 1;
        public double MaxRecommend { get { return maxRec; } set { maxRec = value; Notify("MaxRecommend"); } }
        protected ListView listView;
      
        #region action filters
        protected virtual bool isMultiSelect(object s)
        {
            return SelectCount > 1;
        }
        #endregion
        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ItemsCount = listView.Items.Count;
            if (e.NewItems != null)
                foreach (var item in e.NewItems.OfType<MusicViewModel>())
                {
                    if (item.Recommends > MaxRecommend)
                        MaxRecommend = item.Recommends;
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
                input_filter.Opacity = 1;
                mask_filter.Visibility = Visibility.Collapsed;
            };
            input_filter.LostFocus += (s, e) =>
            {
                input_filter.Opacity = 0.2;
                mask_filter.Visibility = string.IsNullOrEmpty(input_filter.Text) ? Visibility.Visible : Visibility.Collapsed;
            };
        }
        string lastFilter = "";


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
        public virtual void Remove(SongViewModel song)
        {
            UIHelper.RunOnUI(() =>
            {
                items.Remove(song);
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
                MaxRecommend = 1;
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
                try
                {
                    DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Copy);
                }
                catch
                {
                }

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
        protected static MusicViewModelList playList = new MusicViewModelList();
        private int itemsCount;
        public int ItemsCount
        {
            get { return itemsCount; }
            set
            {
                itemsCount = value;
                emptyText.Visibility = value > 0 ? Visibility.Collapsed : Visibility.Visible;
                Notify("ItemsCount");
            }
        }
        private int selectCount;
        public int SelectCount { get { return selectCount; } set { selectCount = value; Notify("SelectCount"); } }
        public IEnumerable<SongViewModel> SelectedSongs
        {
            get
            {
                return ListView.SelectedItems.OfType<SongViewModel>();
            }
            //set
            //{
            //    foreach (var item in SongViewModel.All)
            //    {
            //        item.IsSelected = false;
            //    }
            //    listView.SelectedItem = value.FirstOrDefault();
            //}
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
        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemsCount")
            {
                needsRefreshPlaylist = true;
            }
            if (e.PropertyName == "NowPlaying")
            {
                btn_select_nowplaying_Click(this, null);
            }
        }
        void OnPlayNextMode(string s)
        {
            needsRefreshPlaylist = true;
            ensureRefreshPlayList();
        }
        protected virtual void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var isMultiSel = isMultiSelect(this);
            if (isMultiSel)
                needsRefreshPlaylist = true;
            this.ensureRefreshPlayList(isMultiSel);
            var item = SelectedSongs.FirstOrDefault() ?? Items.OfType<SongViewModel>().FirstOrDefault();
            if (item == null)
                return;
            var location = item.Song.DownloadState == "complete" ? item.Song.FilePath : item.Song.UrlMp3;
            if (!string.IsNullOrEmpty(location))
            {
                Mp3Player.Play(location, item.Id);
                ActionBarService.Refresh();
            }
        }
        protected virtual void btn_next_Click(object sender, RoutedEventArgs e)
        {
            ensureRefreshPlayList();
            Mp3Player.Next();
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

        public virtual void Load()
        {
            items.Load();
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
        private void charmBarAct(object sender, RoutedEventArgs e)
        {
            var sel = (sender as FrameworkElement).DataContext as CharmAction;
            if (sel == null) return;
            sel.Action(sel, null);
            ActionBarService.Refresh();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //foreach (var item in e.RemovedItems.OfType<SongViewModel>())
            //{
            //    item.IsSelected = false;
            //}
            //foreach (var item in e.RemovedItems.OfType<SongViewModel>())
            //{
            //    item.IsSelected = true;
            //}
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

        private void Image_SourceUpdated_1(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var img = sender as Image;
            //Show(img);
        }
        protected void btn_select_nowplaying_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectedItem = SongViewModel.NowPlaying;
            virtualView.ScrollToCenterOfView(SongViewModel.NowPlaying);
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
            if (!XiamiClient.GetDefault().IsLoggedIn) return false;
            var s = (o as SongListControl).SelectedSongs;
            return s.Count() > 0 && s.Any(x => !x.InFav);
        }
        protected static bool canUnfav(object o)
        {
            if (!XiamiClient.GetDefault().IsLoggedIn) return false;
            var s = (o as SongListControl).SelectedSongs;
            return s.Count() > 0 && s.Any(x => x.InFav);
        }
        public static T GetParentOf<T>(DependencyObject o, string name = null) where T : FrameworkElement
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
        private ObservableCollection<CharmAction> itemActionSource1 = new ObservableCollection<CharmAction> { };
        public ObservableCollection<CharmAction> ItemActionSource1
        {
            get { return itemActionSource1; }
            set { itemActionSource1 = value; }
        }
        private ObservableCollection<CharmAction> itemActionSource2 = new ObservableCollection<CharmAction> { };
        public ObservableCollection<CharmAction> ItemActionSource2
        {
            get { return itemActionSource2; }
            set { itemActionSource2 = value; }
        }

        CharmAction open_more = new CharmAction("更多", "\xE0C2", more_Click_1);
        CharmAction close_more = new CharmAction("更少", "\xE0C2", more_Click_2);
        private static void more_Click_1(object sender, RoutedEventArgs e)
        {
        }
        private static void more_Click_2(object sender, RoutedEventArgs e)
        {
        }

        protected Dictionary<string, CharmAction> actions = new Dictionary<string, CharmAction>();
        public CharmAction GetAction(string name)
        {
            if (!actions.ContainsKey(name))
                return null;
            return actions[name];
        }
        public void PerformAction(string name)
        {
            if (!actions.ContainsKey(name))
                return;
            try
            {
                actions[name].Action(null, null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        protected List<CharmAction> contextMenuSource = new List<CharmAction>();
        protected virtual void btn_item_action_Click(object sender, RoutedEventArgs e)
        {
            contextMenuSource.Clear();
            contextMenuSource.AddRange(actions.Values.Where(x => x.Validate(this)));
            var cm = new ContextMenu();
            cm.ItemsSource = contextMenuSource;
            cm.Placement = PlacementMode.Right;
            cm.PreviewMouseUp += cm_PreviewMouseUp;
            cm.PlacementTarget = sender as UIElement;
            cm.StaysOpen = false;
            cm.IsOpen = true;
        }
        void cm_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as ContextMenu).IsOpen = false;
        }
        public IEnumerable<CharmAction> ProvideActions(string barName = "Default")
        {
            var res = new List<CharmAction>();
            if (SelectCount > 1)
                res.AddRange(actions.Values.Where(x => x.Validate(this)));
            return res;
        }
        #region playlist control
        public static bool needsRefreshPlaylist = false;
        protected void ensureRefreshPlayList(bool onlySelected = false)
        {
            if (!needsRefreshPlaylist) return;
            var selSongs = SelectedSongs.ToList();
            playList.Clear();
            var list = onlySelected ? selSongs : Source.OfType<SongViewModel>();
            foreach (var item in list)
            {
                playList.Add(item);
            };
            if (Global.AppSettings["PlayNextMode"] == "Random")
                playList.Shuffle();
            needsRefreshPlaylist = false;
        }
        #endregion
    }
}

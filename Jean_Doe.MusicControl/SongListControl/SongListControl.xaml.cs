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
            if(PropertyChanged != null)
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
                var ui=listView.ItemContainerGenerator.ContainerFromItem(song);
                var storyboard=this.FindResource("FadeOut") as Storyboard;
                Storyboard.SetTarget(storyboard,ui);
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

        MusicViewModelList items;
        public MusicViewModelList Items { get { return items; } }
        private int itemsCount;
        public int ItemsCount { get { return itemsCount; } set { itemsCount = value; Notify("ItemsCount"); } }
        private int selectCount;
        public int SelectCount { get { return selectCount; } set { selectCount = value; Notify("SelectCount"); } }
        public IEnumerable<SongViewModel> SelectedItems
        {
            get
            {
                return listView.SelectedItems.OfType<SongViewModel>();
            }
        }
        //public DataGrid DataGrid { get { return this.dataGrid; } }
        public SongListControl()
        {
            var a = new Image();
            InitializeComponent();
            Global.ListenToEvent("ShowDetails", HandleShowDetails);
            //col_album.Visibility = Visibility.Collapsed;
            MessageBus.Instance.Subscribe(this);
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            listView.ItemsSource = items;
            listView.SelectionChanged += dataGrid_SelectionChanged;
            HandleShowDetails(Global.AppSettings["ShowDetails"]);
        }
        public void UnselectAll()
        {
            listView.UnselectAll();
        }
        void HandleShowDetails(string show)
        {
            if(show == "1")
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
            Task.Run(async () => {await items.Load(); });
        }

        protected virtual void btn_open_click(object sender, RoutedEventArgs e)
        {
            SongViewModel s = null;
            try
            {
                s = (sender as FrameworkElement).DataContext as SongViewModel;
            }
            catch { }
            if(s == null ||!s.CanOpen) return;
            e.Handled = true;
            s.Open();
        }

        protected virtual async void link_album(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasAlbum;
            if(t == null) return;
            var id = t.AlbumId;
            await SearchManager.GetSongOfType(t.AlbumId, EnumMusicType.album);
        }

        protected virtual async void link_artist(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasArtist;
            if(t == null) return;
            await SearchManager.GetSongOfType(t.ArtistId, EnumMusicType.artist);
        }
        protected virtual async void link_collection(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasCollection;
            if(t == null) return;
            await SearchManager.GetSongOfType(t.CollectionId, EnumMusicType.collect);
        }

        protected virtual void go_song(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as MusicViewModel;
            if(t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoSong(t.Id), null);
        }
        protected virtual void go_artist(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasArtist;
            if(t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoArtist(t.ArtistId), null);
        }
        protected virtual void go_album(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as IHasAlbum;
            if(t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoAlbum(t.AlbumId), null);
        }
        protected virtual void go_collect(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as MusicViewModel;
            if(t == null) return;
            RunProgramHelper.RunProgram(XiamiUrl.GoCollect(t.Id), null);
        }
        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UIHelper.RunOnUI(new Action(() =>
            {
                ItemsCount = Items.Count;
                //fitToContent();
            }));
            Save();
        }
        void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIHelper.RunOnUI(new Action(() =>
          {
              SelectCount = SelectedItems.Count();
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

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SongViewModel;
            if(item == null) return;
            PlayItem = item;
        }
        private static SongViewModel playItem = null;

        public static SongViewModel PlayItem
        {
            get { return playItem; }
            set
            {
                if(playItem == value)
                {
                    playItem.TogglePlay();
                    return;
                }
                if(playItem != null)
                    playItem.Stop();
                playItem = value;
                if(playItem != null)
                    playItem.Play();
            }
        }

        private void Image_SourceUpdated_1(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var img=sender as Image;
            //Show(img);
        }

        private static void Show(UIElement obj)
        {
            var da = new DoubleAnimation
            {
                From=0,
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
    }
}

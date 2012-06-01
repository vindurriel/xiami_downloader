using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Artwork.MessageBus;
using Jean_Doe.Common;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for SongListControl.xaml
    /// </summary>
    public partial class SongListControl
    {
        #region list methods
        public virtual void Add(SongViewModel song)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                items.Add(song);
            }));
        }
        public void Remove(SongViewModel song)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                items.Remove(song);
            }));
        }
        public SongViewModel GetItemById(string id)
        {
            var res = items.FirstOrDefault(x => x.Id == id) as SongViewModel;
            return res;
        }
        public void Clear()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                items.Clear();
            }));
        }
        #endregion
        MusicViewModelList items;
        public MusicViewModelList Items { get { return items; } }
        public IEnumerable<SongViewModel> SelectedItems
        {
            get
            {
                return dataGrid.SelectedItems.OfType<SongViewModel>();
            }
        }

        public SongListControl()
        {
            InitializeComponent();
            MessageBus.Instance.Subscribe(this);
            items = new MusicViewModelList();
            items.CollectionChanged += items_CollectionChanged;
            dataGrid.ItemsSource = items;
        }
        public string SavePath { get { return Items.SavePath; } set { Items.SavePath = value; } }
        public virtual void Save()
        {
            items.Save();
        }
        public virtual void Load()
        {
            items.Load();
        }
        public void SetBusy(bool on, bool dim = true)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                busyMask.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
                busyMask.Opacity = dim ? 0.7 : 0.1;
            }));
        }

        protected virtual void btn_open_click(object sender, RoutedEventArgs e)
        {
            var t = sender as FrameworkElement;
            if (t == null) return;
            var x = t.DataContext as SongViewModel;
            if (x == null) return;
            x.Open();
        }

        protected virtual async void link_album(object sender, RoutedEventArgs e) { }

        protected virtual async void link_artist(object sender, RoutedEventArgs e) { }

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                emptyMask.Visibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }));
            Save();
        }
        private void select_all_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.SelectAll();
        }
        private void unselect_all_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.UnselectAll();
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

    }
}

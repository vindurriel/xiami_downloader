using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using System.Collections.Generic;

namespace Jean_Doe.MusicControl
{
    public class SearchSongListControl : SongListControl,
        IHandle<MsgSearchStateChanged>,
        IHandle<MsgSetDescription>
    {
        public SearchSongListControl()
        {
            Items.CollectionChanged += Items_CollectionChanged;
            MessageBus.Instance.Subscribe(this);
            var l = new List<CharmAction> { 
               new CharmAction("下载","\xE118",this.btn_download_add_Click,(s)=>{return (s as SongListControl).SelectedSongs.Count()>0;}),
            };
            foreach (var item in l)
            {
                actions[item.Label] = item;
            }
            addCommonActions();
        }
        private double maxPlayTimes = 1;

        public double MaxPlayTimes
        {
            get { return maxPlayTimes; }
            set
            {
                maxPlayTimes = value;
                Notify("MaxPlayTimes");
            }
        }
        protected override void item_double_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedSongs.Count() > 0)
            {
                if (SelectedSongs.FirstOrDefault().Song.DownloadState == "complete")
                    btn_play_Click(sender, e);
                else
                    btn_download_add_Click(sender, null);
                return;
            }
            base.item_double_click(sender, e);
        }
        void btn_download_add_Click(object sender, RoutedEventArgs e)
        {
            var list = SelectedSongs.ToList();
            var list_download = Artwork.DataBus.DataBus.Get("list_download") as DownloadSongListControl;
            foreach (var item in list)
            {

                list_download.AddAndStart(item);
                Remove(item);
            }
        }
        object lck = new object();
        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }
        public void Handle(MsgSearchStateChanged message)
        {
            switch (message.State)
            {
                case EnumSearchState.Started:
                    Clear();
                    break;
                case EnumSearchState.Working:
                    Items.AddItems(message.SearchResult.Items.ToList());
                    break;
                case EnumSearchState.Finished:
                    break;
                default:
                    break;
            }
        }

        public void Handle(MsgSetDescription message)
        {
            var svm = SongViewModel.GetId(message.Id);
            if (svm == null) return;
            svm.Description = message.Description;
        }
    }
}

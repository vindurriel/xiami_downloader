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
    public class SearchSongListControl : SongListControl, IActionProvider,
        IHandle<MsgSearchStateChanged>
    {
        public SearchSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            Items.CollectionChanged += Items_CollectionChanged;
            listView.ItemTemplate = this.Resources["searchTemplate"] as DataTemplate;
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
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                maxPlayTimes = 1;
                return;
            }
            foreach (var item in e.NewItems.OfType<SongViewModel>())
            {
                //if (item.PlayTimes == 0)
                //{
                //    var bw1 = new BackgroundWorker();
                //    bw1.DoWork += async (a, b) =>
                //        {
                //            var times = await NetAccess.GetPlayTimes(item.Id);
                //            if (times != 0)
                //                UIHelper.RunOnUI(() =>
                //                {
                //                    if (times > MaxPlayTimes)
                //                        MaxPlayTimes = times;
                //                    item.PlayTimes = times;
                //                });
                //        };
                //    bw1.RunWorkerAsync();
                //}
                //if (item.TrackNo == 0)
                //{
                //    var bw2 = new BackgroundWorker();
                //    bw2.DoWork += async (a, b) =>
                //    {
                //        var trackNo = await NetAccess.GetTrackNo(item.Id, item.AlbumId);
                //        if (trackNo != 0)
                //            UIHelper.RunOnUI(() =>
                //            {
                //                item.TrackNo = trackNo;
                //            });
                //    };
                //    bw2.RunWorkerAsync();
                //}
            }
        }
        public void Handle(MsgSearchStateChanged message)
        {
            switch (message.State)
            {
                case EnumSearchState.Started:
                    Clear();
                    break;
                case EnumSearchState.Working:
                    Items.AddItems(message.SearchResult.Items);
                    break;
                case EnumSearchState.Finished:
                    break;
                default:
                    break;
            }
        }

        public System.Collections.Generic.IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction> { 
               new CharmAction("取消选择", btn_cancel_selection_Click,defaultActionValidate),
               new CharmAction("下载",this.btn_download_add_Click,(s)=>{
                   return (s as SongListControl).SelectedSongs.Count()>0;
               }),
               new CharmAction("收藏该歌曲",this.btn_fav_Click,canFav),
               new CharmAction("不再收藏该歌曲",this.btn_unfav_Click,canUnfav),
               new CharmAction("查看专辑的歌曲",link_album,IsOnlyType<IHasAlbum>),
               new CharmAction("查看精选集的歌曲",link_collection,IsOnlyType<CollectViewModel>),
               new CharmAction("查看艺术家的相似艺人",link_similar_artist,IsOnlyType<IHasArtist>),
               new CharmAction("查看艺术家的歌曲",link_artist,IsOnlyType<IHasArtist>),
               new CharmAction("查看艺术家的专辑",link_artist_album,IsOnlyType<IHasArtist>),
               new CharmAction("在浏览器中打开",this.btn_browse_Click,IsOnlyType<IHasMusicPart>),

            };
        }

    }
}

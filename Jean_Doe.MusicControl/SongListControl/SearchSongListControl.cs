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
        IHandle<MsgSearchStateChanged>,
        IHandle<MsgSetDescription>
    {
        public SearchSongListControl()
        {
            Items.CollectionChanged += Items_CollectionChanged;
            wrapView.ItemTemplate = this.Resources["searchTemplate"] as DataTemplate;
            MessageBus.Instance.Subscribe(this);
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
               new CharmAction("取消选择", "\xE10E",btn_cancel_selection_Click,defaultActionValidate),
               new CharmAction("下载","\xE118",this.btn_download_add_Click,(s)=>{
                   return (s as SongListControl).SelectedSongs.Count()>0;
               }),
               new CharmAction("收藏该歌曲","\xE0A5",this.btn_fav_Click,canFav),
               new CharmAction("不再收藏该歌曲","\xE007",this.btn_unfav_Click,canUnfav),
               new CharmAction("查看专辑的歌曲","\xE1d2",link_album,IsOnlyType<IHasAlbum>),
               new CharmAction("查看精选集的歌曲","\xE142",link_collection,IsOnlyType<CollectViewModel>),
               new CharmAction("查看艺术家的歌曲","\xE181",link_artist,IsOnlyType<IHasArtist>),
               new CharmAction("查看艺术家的相似艺人","\xE181相似",link_similar_artist,IsOnlyType<IHasArtist>),
               new CharmAction("查看艺术家的专辑","\xE181专辑",link_artist_album,IsOnlyType<IHasArtist>),
               new CharmAction("在浏览器中打开","\xE12B",this.btn_browse_Click,IsOnlyType<IHasMusicPart>),

            };
        }

        public void Handle(MsgSetDescription message)
        {
           var svm= SongViewModel.GetId(message.Id);
           if (svm == null) return;
           svm.Description = message.Description;
        }
    }
}

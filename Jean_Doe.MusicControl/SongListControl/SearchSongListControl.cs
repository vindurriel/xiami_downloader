using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Xiami;

namespace Jean_Doe.MusicControl
{
    public class SearchSongListControl : SongListControl,
        IHandle<MsgSearchStateChanged>
    {
        public SearchSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            var col = new DataGridTemplateColumn
            {
                CellTemplate = FindResource("PlayTimesTemplate") as DataTemplate,
                Header = "播放次数",
                SortDirection=ListSortDirection.Ascending,
                SortMemberPath = "PlayTimes",
            };
            dataGrid.Columns.Add(col);
            Items.CollectionChanged += Items_CollectionChanged;
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
                if (item.PlayTimes == 0)
                {
                    var bw1 = new BackgroundWorker();
                    bw1.DoWork += async (a, b) =>
                        {
                            var times = await XiamiNetAccess.GetPlayTimes(item.Id);
                            if (times != 0)
                                UIHelper.RunOnUI(() =>
                                {
                                    if (times > MaxPlayTimes)
                                        MaxPlayTimes = times;
                                    item.PlayTimes = times;
                                });
                        };
                    bw1.RunWorkerAsync();
                }
                if (item.TrackNo == 0)
                {
                    var bw2 = new BackgroundWorker();
                    bw2.DoWork += async (a, b) =>
                    {
						var trackNo = await XiamiNetAccess.GetTrackNo(item.Id, item.AlbumId);
                        if (trackNo != 0)
                            UIHelper.RunOnUI(() =>
                            {
                                item.TrackNo = trackNo;
                            });
                    };
                    bw2.RunWorkerAsync();
                }
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
    }


}

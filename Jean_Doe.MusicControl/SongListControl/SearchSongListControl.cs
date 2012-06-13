using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;

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
                SortMemberPath = "PlayTimes",
            };
            dataGrid.Columns.Insert(2, col);
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

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (var item in e.NewItems.OfType<SongViewModel>())
            {
                if (item.PlayTimes == 0)
                    Task.Run(async () =>
                    {
                        var times = await NetAccess.GetPlayTimes(item.Id);
                        UIHelper.RunOnUI(() =>
                        {
                            if (times > MaxPlayTimes) MaxPlayTimes = times;
                            item.PlayTimes = times;
                        });
                    }); 
                if (item.TrackNo == 0)
                    Task.Run(async () =>
                    {
                        var trackNo = await NetAccess.GetTrackNo(item.Id,item.AlbumId);
                        UIHelper.RunOnUI(() =>
                        {
                            item.TrackNo = trackNo;
                        });
                    });
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
namespace Jean_Doe.MusicControl
{
    public class CompleteSongListControl : SongListControl, IHandle<MsgDownloadStateChanged>
    {
        public CompleteSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            //dataGrid.Columns.Add(new DataGridTemplateColumn
            //{
            //    Header = "完成日期",
            //    CellTemplate = dataGrid.FindResource("dateTemplate") as DataTemplate,
            //    SortMemberPath = "Date",
            //    SortDirection = System.ComponentModel.ListSortDirection.Ascending,
            //});
            Items.CollectionChanged += Items_CollectionChanged;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;
            foreach (var item in e.NewItems.OfType<SongViewModel>())
            {
                var file = Path.Combine(Global.BasePath, "cache", item.Id + ".mp3");
                if (File.Exists(file))
                {
                    var date = new FileInfo(file).LastWriteTime;
                    UIHelper.RunOnUI(() =>
                        item.Date = date
                    );
                }
                if (item.Song.FilePath == null)
                {
                    item.Song.FilePath = Path.Combine(Global.AppSettings["DownloadFolder"], item.Dir, item.FileNameBase + ".mp3");
                }
            }
        }
        public void Handle(MsgDownloadStateChanged message)
        {
            var item = message.Item as SongViewModel;
            if (item != null && item.Done)
                Insert(0, item);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Downloader;
namespace Jean_Doe.MusicControl
{
    public class CompleteSongListControl : SongListControl, IHandle<MsgDownloadStateChanged>
    {
        public CompleteSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            //dataGrid.Columns.Insert(0, new DataGridTemplateColumn
            //{
            //    CanUserReorder=false,
            //    CanUserResize=false,
            //    CellTemplate = dataGrid.FindResource("imageTemplate") as DataTemplate,
            //    Width = DataGridLength.SizeToCells,
            //});
        }
        public void Handle(MsgDownloadStateChanged message)
        {
            var item = message.Item as SongViewModel;
            if (item != null && item.AllDone)
                Insert(0, item);
        }
    }
}

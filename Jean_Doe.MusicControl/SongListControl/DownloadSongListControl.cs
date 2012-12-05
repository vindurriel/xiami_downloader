using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using System.Windows.Controls;
using System.Windows;
namespace Jean_Doe.MusicControl
{
    public class DownloadSongListControl : SongListControl,
        IHandle<MsgDownloadProgressChanged>,
        IHandle<MsgDownloadStateChanged>
    {
        public DownloadSongListControl()
        {
            //dataGrid.Columns.Insert(0, new DataGridTemplateColumn
            //{
            //    CellTemplate = dataGrid.FindResource("imageTemplate") as DataTemplate,
            //    Width = DataGridLength.SizeToCells,
            //    CanUserResize=false,
            //    CanUserReorder=false
            //});
            dataGrid.Columns.Add(new DataGridTemplateColumn
            {
                CellTemplate = dataGrid.FindResource("statusTemplate") as DataTemplate,
                Width = DataGridLength.SizeToCells,
                Header = "状态",
                SortMemberPath = "Status"
            });
            Items.CollectionChanged+=Items_CollectionChanged;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
                foreach (var item in e.NewItems.OfType<SongViewModel>())
                {
                    item.PrepareDownload();
                }
            }
        }
        public  void AddAndStart(SongViewModel song)
        {
			Add(song);
            song.PrepareDownload();
            DownloadManager.Instance.StartByTag(new List<string> { song.Id });
        }
        public void Handle(MsgDownloadProgressChanged message)
        {
            var id = message.Id;
            var item = GetItemById(id);
            if(item == null) return;
            UIHelper.RunOnUI(new Action(() =>
            {
                item.SetProgress(message.Percent);
            }));
        }
        public void Handle(MsgDownloadStateChanged message)
        {
            var item = message.Item as SongViewModel;
            if(item == null) return;
            if(item.Done)
            {
                Remove(item);
                return;
            }
            var msg = message.Message;
            if(msg == null)
            {
                switch(message.State)
                {
                    case EnumDownloadState.StandBy:
                        msg = "停止";
                        break;
                    case EnumDownloadState.Waiting:
                        msg = "正在开始";
                        break;
                    case EnumDownloadState.Downloading:
                        msg = "正在下载";
                        break;
                    case EnumDownloadState.Cancel:
                        msg = "下载暂停";
                        break;
                    case EnumDownloadState.Success:
                        break;
                    case EnumDownloadState.Error:
                        msg = "有错误";
                        break;
                    default:
                        break;
                }
            }

            UIHelper.RunOnUI(new Action(() =>
            {
                item.Status = msg;
            }));
        }
    }
}

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
    public class DownloadSongListControl : SongListControl,IActionProvider,
        IHandle<MsgDownloadProgressChanged>,
        IHandle<MsgDownloadStateChanged>
    {
        public DownloadSongListControl()
        {
            listView.ItemTemplate = this.Resources["downloadingTemplate"] as DataTemplate;

            //dataGrid.Columns.Insert(0, new DataGridTemplateColumn
            //{
            //    CellTemplate = dataGrid.FindResource("imageTemplate") as DataTemplate,
            //    Width = DataGridLength.SizeToCells,
            //    CanUserResize=false,
            //    CanUserReorder=false
            //});

            //dataGrid.Columns.Add(new DataGridTemplateColumn
            //{
            //    CellTemplate = dataGrid.FindResource("statusTemplate") as DataTemplate,
            //    Width = DataGridLength.SizeToCells,
            //    Header = "状态",
            //    SortMemberPath = "Status"
            //});
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

        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction> 
                { 
                    new CharmAction("取消选择",this.btn_cancel_selection_Click,defaultActionValidate),
                    new CharmAction("开始",this.btn_download_start_Click,defaultActionValidate),
                    new CharmAction("暂停",this.btn_cancel_Click,defaultActionValidate),
                    new CharmAction("删除",this.btn_remove_Click,defaultActionValidate),
                    new CharmAction("完成",this.btn_complete_Click,defaultActionValidate),
                    new CharmAction("查看专辑歌曲",link_album,IsType<IHasAlbum>),
                    new CharmAction("查看歌手歌曲",link_artist,IsType<IHasArtist>),
                };
        }

        void btn_download_start_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StartByTag(SelectedSongs.Select(x => x.Id).ToList());
        }

        void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StopByTag(SelectedSongs.Select(x => x.Id).ToList());
        }
        void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.RemoveByTag(SelectedSongs.Select(x => x.Id).ToList());
            var list = SelectedSongs.ToList();
            foreach (var item in list)
            {
                Remove(item);
            }
        }

        void btn_complete_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedSongs.Any())
                return;
            DownloadManager.Instance.StopByTag(SelectedSongs.Select(x => x.Id).ToList());
            foreach (var item in SelectedSongs)
            {
                item.HasMp3 = true; item.HasLrc = true; item.HasArt = true;
                MessageBus.Instance.Publish(new MsgDownloadStateChanged
                {
                    Id = item.Id,
                    Item = item,
                });
            }
        }
    }
}

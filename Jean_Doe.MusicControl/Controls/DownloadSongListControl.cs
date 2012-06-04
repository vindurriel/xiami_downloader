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
        }
        public override void Load()
        {
            base.Load();
            PrepareDownload(Items.OfType<SongViewModel>());
        }
        public override void Add(SongViewModel song)
        {
            base.Add(song);
            PrepareDownload(song);
        }
        public  void AddAndStart(SongViewModel song)
        {
            base.Add(song);
            PrepareDownload(song);
            DownloadManager.Instance.Start(new List<string> { song.Id });
        }
        public static void PrepareDownload(IEnumerable<SongViewModel> list)
        {
            foreach(var item in list)
            {
                PrepareDownload(item);
            }
        }
        public static void PrepareDownload(SongViewModel item)
        {
            var nameBase = System.IO.Path.Combine(Global.BasePath, "cache", item.Id);
            if(!item.HasMp3)
            {
                var mp3 = nameBase + ".mp3";
                var d = new DownloaderMp3
                {
                    Info = new DownloaderInfo
                    {
                        Id = "mp3" + item.Id,
                        FileName = mp3,
                        Entity = item,
                        Tag = item.Id,
                    }
                };
                DownloadManager.Instance.Add(d);
            }
            if(!item.HasLrc)
            {
                var lrc = nameBase + ".lrc";
                var d = new DownloaderLrc
                {
                    Info = new DownloaderInfo
                    {
                        Id = "lrc" + item.Id,
                        FileName = lrc,
                        Tag = item.Id,
                        Entity = item
                    }
                };
                DownloadManager.Instance.Add(d);
            }
            if(!item.HasArt)
            {
                var art = System.IO.Path.Combine(
                    Global.BasePath, "cache",
                    item.AlbumId + ".art");
                var d = new DownloaderArt
                {
                    Info = new DownloaderInfo
                    {
                        Id = "art" + item.AlbumId,
                        FileName = art,
                        Entity = item,
                        Tag = item.Id,
                    }
                };
                var first = DownloadManager.Instance.GetDownloader(d.Info.Id) as DownloaderArt;
                if(first == null)
                    DownloadManager.Instance.Add(d);
                else
                {
                    first.ArtDownloaded += d.HandleDownloaded;
                }
            }
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
            if(item.AllDone)
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
                        msg = "下载取消";
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

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
            Items.CollectionChanged += Items_CollectionChanged;
            MessageBus.Instance.Subscribe(this);
            var l = new List<CharmAction> { 
                    new CharmAction("取消选择","\xE10E",this.btn_cancel_selection_Click,defaultActionValidate),
                    new CharmAction("开始","\xE118",this.btn_download_start_Click,defaultActionValidate),
                    new CharmAction("暂停","\xE15B",this.btn_cancel_Click,defaultActionValidate),
                    new CharmAction("删除","\xE106",this.btn_remove_Click,defaultActionValidate),
                    new CharmAction("完成","\xE10B",this.btn_complete_Click,defaultActionValidate),
                    new CharmAction("查看专辑歌曲","\xE1d2",link_album,IsOnlyType<IHasAlbum>),
                    new CharmAction("查看艺术家","\xe13d",link_artist,IsOnlyType<IHasArtist>),
                    new CharmAction("在浏览器中打开","\xE12B",this.btn_browse_Click,IsOnlyType<IHasMusicPart>),
                };
            foreach (var item in l)
            {
                actions[item.Label] = item;
                contextMenuSource.Add(item);
            }
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.OfType<SongViewModel>())
                {
                    item.PrepareDownload();
                }
            }
        }
        public void AddAndStart(SongViewModel song)
        {
            Items.Add(song);
            var hasWork = DownloadManager.Instance.StartByTag(new string[] { song.Id });
            if (!hasWork)
                MessageBus.Instance.Publish(new MsgDownloadStateChanged { Id = song.Id, Item = song, State = EnumDownloadState.Success });
        }
        public void Handle(MsgDownloadProgressChanged message)
        {
            var id = message.Id;
            var item = GetItemById(id);
            if (item == null) return;
            UIHelper.RunOnUI(new Action(() =>
            {
                item.SetProgress(message.Percent);
            }));
        }
        public void Handle(MsgDownloadStateChanged message)
        {
            var item = message.Item as SongViewModel;
            if (item == null) return;
            if (item.HasMp3)
            {
                Remove(item);
                return;
            }
            var msg = message.Message;
            if (msg == null)
            {
                switch (message.State)
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
                        msg = "";
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
                item.Description = msg;
            }));
        }

        void btn_download_start_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StartByTag(SelectedSongs.Select(x => x.Id).ToArray());
        }

        void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.StopByTag(SelectedSongs.Select(x => x.Id).ToArray());
        }
        void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.Instance.RemoveByTag(SelectedSongs.Select(x => x.Id).ToArray());
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
            DownloadManager.Instance.StopByTag(SelectedSongs.Select(x => x.Id).ToArray());
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

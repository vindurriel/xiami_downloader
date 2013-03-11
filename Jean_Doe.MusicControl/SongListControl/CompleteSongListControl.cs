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
    public class CompleteSongListControl : SongListControl, IHandle<MsgDownloadStateChanged>, IActionProvider
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

        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction> 
                {   
                    new CharmAction("播放/暂停",this.btn_play_Click,(s)=>{
                        return (s as CompleteSongListControl).SelectCount == 1;
                    }),    
                    new CharmAction("查看专辑歌曲",link_album,IsType<IHasAlbum>),
                    new CharmAction("查看歌手歌曲",link_artist,IsType<IHasArtist>),
                    new CharmAction("存为播放列表",this.btn_save_playlist_Click,defaultActionValidate),
                    new CharmAction("复制文件到剪贴板",this.btn_copy_Click,defaultActionValidate),
                    new CharmAction("删除",this.btn_remove_complete_Click,defaultActionValidate),
                    new CharmAction("取消选择",this.btn_cancel_selection_Click,defaultActionValidate),
                };
        }

        void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var slider = Artwork.DataBus.DataBus.Get("slider") as Slider;
            slider.Visibility = Visibility.Visible;
            foreach (var item in SelectedSongs)
            {
                if (!item.HasMp3) continue;
                if (!string.IsNullOrEmpty(item.Song.FilePath))
                {
                    Mp3Player.PlayPause(item.Song.FilePath);
                }
            }
        }

        void btn_save_playlist_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedSongs.Any())
                return;
            var win = new System.Windows.Forms.SaveFileDialog
            {
                InitialDirectory = Global.AppSettings["DownloadFolder"],
                Filter = "播放列表文件 (*.m3u)|*.m3u",
                OverwritePrompt = true,
                Title = "存为播放列表"
            };
            if (win.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SavePlaylist(win.FileName);
            }
        }
        void btn_remove_complete_Click(object sender, RoutedEventArgs e)
        {
            var list = SelectedSongs.ToList();
            foreach (var item in list)
            {
                Remove(item);
            }
        }
        void btn_copy_Click(object sender, RoutedEventArgs e)
        {
            var files = new System.Collections.Specialized.StringCollection();
            foreach (var item in SelectedSongs)
            {
                if (string.IsNullOrEmpty(item.Song.FilePath))
                    files.Add(System.IO.Path.Combine(".", item.Dir, item.FileNameBase + ".mp3"));
                else
                    files.Add(item.Song.FilePath);
            }
            if (files.Count > 0)
                Clipboard.SetFileDropList(files);
        }
        List<string> Playlist = new List<string>();
        string SavePlaylist(string path = null)
        {
            if (!SelectedSongs.Any())
                return null;
            Playlist.Clear();
            foreach (var item in SelectedSongs)
            {
                if (!item.HasMp3) continue;
                Playlist.Add(string.Format("#EXTINF:{0}", item.Id));
                if (string.IsNullOrEmpty(item.Song.FilePath))
                    Playlist.Add(System.IO.Path.Combine(".", item.Dir, item.FileNameBase + ".mp3"));
                else
                    Playlist.Add(item.Song.FilePath);
            }
            if (path == null)
                path = Global.AppSettings["DownloadFolder"] + "\\default.m3u";
            File.WriteAllLines(path, Playlist, Encoding.UTF8);
            return path;
        }
    }
}

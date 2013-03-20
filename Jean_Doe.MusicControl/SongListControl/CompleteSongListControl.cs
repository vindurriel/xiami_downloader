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
    public class CompleteSongListControl : SongListControl, IHandle<MsgDownloadStateChanged>, IHandle<MsgRequestNextSong>, IActionProvider
    {
        public CompleteSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            Items.CollectionChanged += Items_CollectionChanged;
            now_playing.Visibility = Visibility.Visible;
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
                    new CharmAction("取消选择",this.btn_cancel_selection_Click,defaultActionValidate),
                    new CharmAction("播放/暂停",this.btn_play_Click,(s)=>{
                        return (s as CompleteSongListControl).SelectCount == 1;
                    }),  
                    new CharmAction("正在播放",this.btn_show_Click,(s)=>{
                        var list = s as CompleteSongListControl;
                        if (list.NowPlaying == null) return false;
                        var song=list.SelectedSongs.FirstOrDefault();
                        return !Object.ReferenceEquals(list.NowPlaying, song);
                    }),  
                    new CharmAction("下一首",this.btn_next_Click,defaultActionValidate),    
                    new CharmAction("查看专辑歌曲",link_album,IsType<IHasAlbum>),
                    new CharmAction("查看歌手歌曲",link_artist,IsType<IHasArtist>),
                    new CharmAction("存为播放列表",this.btn_save_playlist_Click,s=>(s as CompleteSongListControl).SelectedSongs.Count()>1),
                    new CharmAction("复制文件到剪贴板",this.btn_copy_Click,defaultActionValidate),
                    new CharmAction("删除",this.btn_remove_complete_Click,defaultActionValidate),
                };
        }
        void btn_show_Click(object sender, RoutedEventArgs e)
        {
            SelectedSongs = new SongViewModel[] { NowPlaying as SongViewModel };
            listView.ScrollIntoView(NowPlaying);
            ActionBarService.Refresh();
        }
        void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var slider = Artwork.DataBus.DataBus.Get("slider") as Slider;
            slider.Visibility = Visibility.Visible;
            var item = SelectedSongs.FirstOrDefault();
            if (item==null || !item.HasMp3) return;
            if (!string.IsNullOrEmpty(item.Song.FilePath))
            {
                Mp3Player.PlayPause(item.Song.FilePath);
                NowPlaying = item;
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

        public void Handle(MsgRequestNextSong message)
        {
            if (Items.Count == 0) return;
            SongViewModel item = null;
            var now = (NowPlaying as SongViewModel) ?? SelectedSongs.FirstOrDefault() ;
            var mode = EnumPlayNextMode.Random;
            Enum.TryParse(Global.AppSettings["PlayNextMode"], out mode);
            switch (mode)
            {
                case EnumPlayNextMode.Sequential:
                     int i = Items.IndexOf(now);
                if (i == -1 || i >= Items.Count) return;
                i = i == Items.Count - 1 ? 0 : i + 1;
                item = Items.OfType<SongViewModel>().ToList().ElementAt(i);
                    break;
                case EnumPlayNextMode.Random:
                    var r = new Random().Next(Items.Count);
                item = Items.OfType<SongViewModel>().ToList().ElementAt(r);
                    break;
                case EnumPlayNextMode.Repeat:
                    item = now;
                    break;
                default:
                    break;
            }
            if (item == null || !item.HasMp3 || string.IsNullOrEmpty(item.Song.FilePath)) return;
            message.Next = item.Song.FilePath;
            NowPlaying = item;
            SelectedSongs = new SongViewModel[] { item };
            listView.ScrollIntoView(item);
            ActionBarService.Refresh();
        }
        void btn_next_Click(object sender, RoutedEventArgs e)
        {
            var msg = new MsgRequestNextSong();
            Handle(msg);
            if (msg.Next == null)
                return;
            var slider = Artwork.DataBus.DataBus.Get("slider") as Slider;
            slider.Visibility = Visibility.Visible;
            Mp3Player.PlayPause(msg.Next);
        }
    }
}

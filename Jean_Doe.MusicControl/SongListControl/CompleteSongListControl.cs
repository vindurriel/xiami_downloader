using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Jean_Doe.MusicControl
{
    public class CompleteSongListControl : SongListControl,
        IHandle<MsgDownloadStateChanged>,
        IHandle<MsgRequestNextSong>, IActionProvider
    {
        public CompleteSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
            Mp3Player.SongChanged += Mp3Player_SongChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            watcher = CreateWatcher();
        }
        static FileSystemWatcher watcher;
        FileSystemWatcher CreateWatcher()
        {
            using (var s = File.CreateText(Path.Combine(Global.BasePath, "a.txt"))) { }
            var f = new FileSystemWatcher(Global.BasePath, "a.txt") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
            f.Changed += f_Changed;
            return f;
        }
        void f_Changed(object sender, FileSystemEventArgs e)
        {
            while (true)
            {
                try
                {
                    using (var s = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                    }
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            var cmd = File.ReadAllText(e.FullPath);
            if (cmd == "next")
                UIHelper.RunOnUI(() => btn_next_Click(null, null));
            else if (cmd == "pause")
                UIHelper.RunOnUI(() => btn_play_Click(null, null));
            UIHelper.RunOnUI(() => ActionBarService.Refresh());
        }

        void Mp3Player_SongChanged(object sender, SongChangedEventArgs e)
        {
            NowPlaying = Items.FirstOrDefault(x => x.Id == e.Id) as SongViewModel;
            ActionBarService.Refresh();
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
            if (item == null || !item.HasMp3)
                return;
            Items.AddItems(new List<IMusic> { item.Song }, true);
        }

        public IEnumerable<CharmAction> ProvideActions()
        {
            return new List<CharmAction> 
                {   
                    new CharmAction("导入",this.btn_import_click,s=>true),
                    new CharmAction("取消选择",this.btn_cancel_selection_Click,defaultActionValidate),
                    new CharmAction("播放",this.btn_play_Click,(s)=>{
                        if (SelectCount == 0)
                            return false;
                        if (!Mp3Player.IsPlaying) 
                            return true;
                        if (isNowPlayingNotSelected(s))
                            return true;
                        return false;
                    }),
                    new CharmAction("暂停",this.btn_play_Click,(s)=>{
                        if (SelectCount == 0)
                            return false;
                        if (isNowPlayingSelected(s) && Mp3Player.IsPlaying)
                            return true;
                        return false;
                    }),
                    new CharmAction("正在播放",this.btn_show_Click,isNowPlayingNotSelected),  
                    new CharmAction("下一首",this.btn_next_Click,isNowPlayingSelected),    
                    new CharmAction("收藏该歌曲",this.btn_fav_Click,canFav),
                    new CharmAction("不再收藏该歌曲",this.btn_unfav_Click,canUnfav),
                    new CharmAction("查看专辑歌曲",link_album,IsOnlyType<IHasAlbum>),
                    new CharmAction("查看艺术家歌曲",link_artist,IsOnlyType<IHasArtist>),
                    new CharmAction("存为播放列表",this.btn_save_playlist_Click,s=>(s as CompleteSongListControl).SelectedSongs.Count()>1),
                    new CharmAction("复制文件到剪贴板",this.btn_copy_Click,defaultActionValidate),
                    new CharmAction("打开文件所在位置",this.btn_open_click,IsOnlyType<IHasMusicPart>),
                    new CharmAction("在浏览器中打开",this.btn_browse_Click,IsOnlyType<IHasMusicPart>),

                    new CharmAction("删除",this.btn_remove_complete_Click,defaultActionValidate),
                };
        }

        bool isNowPlayingSelected(object s)
        {
            var list = s as CompleteSongListControl;
            var song = list.SelectedSongs.FirstOrDefault();
            if (song == null) return false;
            if (list.NowPlaying == null) return true;
            return Object.ReferenceEquals(list.NowPlaying, song);
        }
        bool isNowPlayingNotSelected(object s)
        {
            var list = s as CompleteSongListControl;
            if (list.NowPlaying == null) return false;
            var song = list.SelectedSongs.FirstOrDefault();
            return !Object.ReferenceEquals(list.NowPlaying, song);
        }

        void btn_show_Click(object sender, RoutedEventArgs e)
        {
            SelectedSongs = new SongViewModel[] { NowPlaying as SongViewModel };
            listView.ScrollIntoView(NowPlaying);
        }
        protected override void btn_play_Click(object sender, RoutedEventArgs e)
        {
            var item = SelectedSongs.FirstOrDefault();
            if (!string.IsNullOrEmpty(item.Song.FilePath))
            {
                Mp3Player.Play(item.Song.FilePath, item.Id);
                ActionBarService.Refresh();
            }
        }
        static System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^(\\d+) (\\d+) (\\d+)$");
        void btn_import_click(object sender, RoutedEventArgs e)
        {
            var dir = Global.AppSettings["DownloadFolder"];
            Task.Run(() =>
            {
                var songs = new List<IMusic>();
                var files = Directory.EnumerateFiles(dir, "*.mp3").ToArray();
                var i = 0;
                foreach (var item in files)
                {
                    try
                    {
                        var mp3 = TagLib.File.Create(item).Tag;
                        if (mp3.Comment == null) return;
                        var m = reg.Match(mp3.Comment);
                        if (!m.Success) continue;
                        var song = new Song
                        {
                            Id = m.Groups[1].Value,
                            ArtistId = m.Groups[2].Value,
                            AlbumId = m.Groups[3].Value,
                            Name = mp3.Title,
                            ArtistName = mp3.FirstPerformer,
                            AlbumName = mp3.Album,
                            FilePath = item,
                        };
                        songs.Add(song);
                        i += 1;
                    }
                    catch (Exception ex)
                    {

                    }
                    if (i == 10)
                    {
                        Items.AddItems(songs);
                        songs.Clear();
                        i = 0;
                    }
                }
                Items.AddItems(songs);
            });

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
            var now = NowPlaying ?? SelectedSongs.FirstOrDefault() as SongViewModel;
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
                    Mp3Player.CurrentTime = TimeSpan.Zero;
                    break;
                default:
                    break;
            }
            if (item == null || !item.HasMp3 || string.IsNullOrEmpty(item.Song.FilePath)) return;
            message.Next = item.Song.FilePath;
            message.Id = item.Id;
            SelectedSongs = new SongViewModel[] { item };
            listView.ScrollIntoView(item);
        }
        void btn_next_Click(object sender, RoutedEventArgs e)
        {
            Mp3Player.Next();
        }
    }
}

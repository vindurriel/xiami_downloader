using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            wrapView.DataContext = null;
            Mp3Player.SongChanged += Mp3Player_SongChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            watcher = CreateWatcher();
            MessageBus.Instance.Subscribe(this);
            Global.ListenToEvent("PlayNextMode", OnPlayNextMode);
            this.PropertyChanged += OnPropertyChanged;
            combo_sort.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "播放顺序", Tag = "Playlist_Asc" });
            combo_sort.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "最近下载", Tag = "Date_Dsc" });
        }

        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemsCount")
            {
                needsRefreshPlaylist = true;
            }
            if (e.PropertyName == "NowPlaying")
            {
                btn_select_nowplaying_Click(this, null);
            }
        }
        void OnPlayNextMode(string s)
        {
            needsRefreshPlaylist = true;
            ensureRefreshPlayList();
        }
        void ensureRefreshPlayList(bool onlySelected = false)
        {
            if (!needsRefreshPlaylist) return;
            var selSongs = SelectedSongs.ToList();
            playList.Clear();
            var list = onlySelected ? selSongs : Source.OfType<SongViewModel>();
            foreach (var item in list)
            {
                playList.Add(item);
            };
            if (Global.AppSettings["PlayNextMode"] == "Random")
                playList.Shuffle();
            needsRefreshPlaylist = false;
        }

        public override void Remove(SongViewModel song)
        {
            base.Remove(song);
            PersistHelper.Delete(song.Song);
        }

        protected override void ApplyFilter()
        {
            base.ApplyFilter();
            ensureRefreshPlayList();
        }

        protected override void ApplySort()
        {
            var tag = (combo_sort.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag.ToString();
            if (tag == "Playlist_Asc")
            {
                virtualView.DataContext = playList;
                GongSolutions.Wpf.DragDrop.DragDrop.SetIsDragSource(virtualView, true);
                GongSolutions.Wpf.DragDrop.DragDrop.SetIsDropTarget(virtualView, true);
                return;
            }
            GongSolutions.Wpf.DragDrop.DragDrop.SetIsDragSource(virtualView, false);
            GongSolutions.Wpf.DragDrop.DragDrop.SetIsDropTarget(virtualView, false);
            virtualView.DataContext = Source;
            base.ApplySort();
            btn_select_nowplaying_Click(null, null);
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
                var file = item.Song.FilePath;
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
                    new CharmAction("取消选择","\xE10E",this.btn_cancel_selection_Click,defaultActionValidate),
                    new CharmAction("播放","\xE102",this.btn_play_Click,(s)=>{
                        if (SelectCount == 0)
                            return false;
                        if (!Mp3Player.IsPlaying) 
                            return true;
                        if (isNowPlayingNotSelected(s))
                            return true;
                        return false;
                    }),
                    new CharmAction("暂停","\xE103",this.btn_play_Click,(s)=>{
                        if (SelectCount == 0)
                            return false;
                        if (isNowPlayingSelected(s) && Mp3Player.IsPlaying)
                            return true;
                        return false;
                    }),
                    new CharmAction("选中正在播放","\xE18B",this.btn_select_nowplaying_Click,isNowPlayingNotSelected),  
                    new CharmAction("下一首","\xE101",this.btn_next_Click,isNowPlayingSelected),    
                    new CharmAction("收藏该歌曲","\xE0A5",this.btn_fav_Click,canFav),
                    new CharmAction("不再收藏该歌曲","\xE007",this.btn_unfav_Click,canUnfav),
                    new CharmAction("查看专辑","\xE1d2",link_album,IsOnlyType<IHasAlbum>),
                    new CharmAction("查看艺术家","\xe13d",link_artist,IsOnlyType<IHasArtist>),
                    new CharmAction("存为播放列表","\xE14C",this.btn_save_playlist_Click,s=>(s as CompleteSongListControl).SelectedSongs.Count()>1),
                    new CharmAction("复制文件到剪贴板","\xE16F",this.btn_copy_Click,defaultActionValidate),
                    new CharmAction("打开文件所在位置","\xE1A5",this.btn_open_click,IsOnlyType<IHasMusicPart>),
                    new CharmAction("在浏览器中打开","\xE12B",this.btn_browse_Click,IsOnlyType<IHasMusicPart>),
                    new CharmAction("删除","\xE106",this.btn_remove_complete_Click,defaultActionValidate),
                    new CharmAction("导入","\xE150",this.btn_import_click,s=>{return ItemsCount==0 || defaultActionValidate(s);}),
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
            return song != null && !Object.ReferenceEquals(list.NowPlaying, song);
        }

        void btn_select_nowplaying_Click(object sender, RoutedEventArgs e)
        {
            SelectedSongs = new SongViewModel[] { NowPlaying as SongViewModel };
            virtualView.ScrollToCenterOfView(NowPlaying);
        }
        bool needsRefreshPlaylist = false;
        protected override void item_double_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btn_play_Click(sender, e);
            ActionBarService.Refresh();
        }
        protected void btn_play_Click(object sender, RoutedEventArgs e)
        {
            bool isMultiSel = SelectedSongs.Count() > 1;
            if (isMultiSel)
                needsRefreshPlaylist = true;
            this.ensureRefreshPlayList(isMultiSel);
            var item = SelectedSongs.FirstOrDefault() ?? Items.OfType<SongViewModel>().FirstOrDefault();
            if (item == null)
                return;
            if (!string.IsNullOrEmpty(item.Song.FilePath))
            {
                Mp3Player.Play(item.Song.FilePath, item.Id);
                ActionBarService.Refresh();
            }
        }
        Regex reg = new Regex("^(\\d+)$");
        Regex reg_ids = new Regex("^(\\d+) (\\d+) (\\d+)$");
        void btn_import_click(object sender, RoutedEventArgs e)
        {
            var dir = Global.AppSettings["DownloadFolder"];
            Task.Run(async () =>
            {
                var buffer = new List<IMusic>();
                int bufferLength = 10;
                var mp3s = Directory.EnumerateFiles(dir, "*.mp3").ToArray();
                foreach (var item in mp3s)
                {
                    try
                    {
                        var mp3 = TagLib.File.Create(item);
                        var tags = mp3.Tag;
                        if (tags.Comment == null) continue;
                        var id = "";
                        var artistid = "";
                        var albumid = "";
                        var logo = "";
                        var m = reg_ids.Match(tags.Comment);
                        if (!m.Success)
                        {
                            m = reg.Match(tags.Comment);
                            if (!m.Success) continue;
                            id = m.Groups[1].Value;
                            var obj = await XiamiClient.GetDefault().Call_xiami_api("Songs.detail", "id=" + id);
                            artistid = MusicHelper.Get(obj["song"], "artist_id");
                            albumid = MusicHelper.Get(obj["song"], "album_id");
                            logo = StringHelper.EscapeUrl(MusicHelper.Get(obj["song"], "logo"));
                            tags.Comment = string.Join(" ", new[] { id, artistid, albumid });
                            mp3.Save();
                        }
                        else
                        {
                            id = m.Groups[1].Value;
                            artistid = m.Groups[2].Value;
                            albumid = m.Groups[3].Value;
                        }
                        var art = System.IO.Path.Combine(Global.BasePath, "cache", albumid + ".art");
                        if (!File.Exists(art))
                        {
                            if (string.IsNullOrEmpty(logo))
                            {
                                var obj = await XiamiClient.GetDefault().Call_xiami_api("Songs.detail", "id=" + id);
                                logo = StringHelper.EscapeUrl(MusicHelper.Get(obj["song"], "logo"));
                            }
                            await new System.Net.WebClient().DownloadFileTaskAsync(logo, art);
                        }
                        var song = new Song
                        {
                            Id = id,
                            ArtistId = artistid,
                            AlbumId = albumid,
                            Name = tags.Title,
                            ArtistName = tags.FirstPerformer,
                            AlbumName = tags.Album,
                            FilePath = item,
                            Logo = logo,
                        };
                        buffer.Add(song);
                    }
                    catch (Exception ex)
                    {
                    }
                    if (buffer.Count == bufferLength)
                    {
                        var songs = new List<IMusic>();
                        foreach (var s in buffer.OfType<Song>())
                        {
                            SongViewModel.Get(s).HasMp3 = true;
                        }
                        songs.AddRange(buffer);
                        Items.AddItems(songs);
                        buffer.Clear();
                    }
                }
                if (buffer.Count > 0)
                {
                    foreach (var s in buffer.OfType<Song>())
                    {
                        SongViewModel.Get(s).HasMp3 = true;
                    }
                    Items.AddItems(buffer);
                }
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
            Task.Run(() =>
            {
                foreach (var item in list)
                {

                    Remove(item);
                    try
                    {
                        File.Delete(item.Song.FilePath);
                        File.Delete(Path.Combine(Global.BasePath, "cache", item.Id + ".mp3"));
                    }
                    catch (Exception ex)
                    {
                    }
                    
                    SongViewModel.Remove(item.Id);
                    System.Threading.Thread.Sleep(10);
                }
            });
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
            var now = NowPlaying ?? SelectedSongs.FirstOrDefault() ?? Items.OfType<SongViewModel>().FirstOrDefault();
            var mode = EnumPlayNextMode.Random;
            Enum.TryParse(Global.AppSettings["PlayNextMode"], out mode);
            switch (mode)
            {
                case EnumPlayNextMode.Sequential:
                case EnumPlayNextMode.Random:
                    if (playList.Count == 0) break;
                    int i = playList.IndexOf(now);
                    if (i == -1) i = playList.Count - 1;
                    i = i == playList.Count - 1 ? 0 : i + 1;
                    item = playList.ElementAt(i) as SongViewModel;
                    break;
                case EnumPlayNextMode.Repeat:
                    item = now;
                    Mp3Player.CurrentTime = 0.0;
                    break;
                default:
                    break;
            }
            if (item == null || string.IsNullOrEmpty(item.Song.FilePath)) return;
            message.Next = item.Song.FilePath;
            message.Id = item.Id;
            SelectedSongs = new SongViewModel[] { item };
            listView.ScrollToCenterOfView(item);
        }
        void btn_next_Click(object sender, RoutedEventArgs e)
        {
            ensureRefreshPlayList();
            Mp3Player.Next();
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jean_Doe.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System;
namespace Jean_Doe.MusicControl
{
    public class MusicViewModelList : ObservableCollection<MusicViewModel>
    {
        ConcurrentQueue<IMusic> queue = new ConcurrentQueue<IMusic>();
        public void AddItems(List<IMusic> inlist, bool toFront = false)
        {
            int count = inlist.Count();
            if (count == 0) return;
            if (count > 10) count = 10;
            int s = 1000 / count;
            Task.Run(() =>
            {
                try
                {
                    foreach (var item in inlist)
                    {
                        queue.Enqueue(item);
                    }
                    for (int i = 0; i < count; i++)
                    {
                        IMusic item = null;
                        if (!queue.TryDequeue(out item)) break;
                        addItem(item, toFront);
                        Thread.Sleep(s);
                    }
                    int buffer = 0;
                    while (true)
                    {
                        IMusic item = null;
                        if (!queue.TryDequeue(out item)) break;
                        addItem(item, toFront);
                        buffer++;
                        if (buffer == 10000)
                        {
                            Thread.Sleep(100);
                            buffer = 0;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    var x = e.StackTrace;
                }
            });
        }
        public new void Remove(MusicViewModel item)
        {
            base.Remove(item);
        }

        void addItem(IMusic music, bool toFront)
        {
            MusicViewModel s = createViewModel(music, toFront);
            if (s == null) return;
            UIHelper.WaitOnUI(() =>
            {
                if (s is SongViewModel)
                {
                    var dup = this.IndexOf(s);
                    if (dup != -1)
                    {
                        Move(dup, toFront ? 0 : this.Count - 1);
                        return;
                    }
                }
                if (toFront)
                    Insert(0, s);
                else
                    Add(s);
            });
        }
        private MusicViewModel createViewModel(IMusic music, bool toFront)
        {
            MusicViewModel s = null;
            switch (music.Type)
            {
                case EnumMusicType.album:
                    s = new AlbumViewModel(music as Album);
                    break;
                case EnumMusicType.artist:
                    s = new ArtistViewModel(music as Artist);
                    break;
                case EnumMusicType.collect:
                    s = new CollectViewModel(music as Collection);
                    break;
                default:
                    s = SongViewModel.Get(music as Song);
                    break;
            }

            return s;
        }
        public string SavePath { get; set; }
        public void Load()
        {
            try
            {
                var songs = SongViewModel.All
                    .Where(x => x.Song.DownloadState == SavePath)
                    .OrderBy(x => x.Date)
                    .Reverse()
                    .Select(x => x.Song as IMusic)
                    .ToList();
                AddItems(songs);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
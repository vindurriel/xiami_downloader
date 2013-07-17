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
        public void AddItems(IEnumerable<IMusic> inlist, bool toFront = false)
        {
            canSave = false;
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

                canSave = true;
                Save();
            });
        }
        public new void Remove(MusicViewModel item)
        {
            canSave = false;
            base.Remove(item);
            canSave = true;
            Save();
        }
        bool canSave = true;
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
        public void Save()
        {
            if (!canSave) return;
            if (SavePath == null) return;
            var songs = this.OfType<SongViewModel>().Select(x => x.Song).ToList();
            try
            {
                string downloadState = System.IO.Path.GetFileNameWithoutExtension(SavePath);
                using (var db = new SQLite.SQLiteConnection(PersistHelper.SqliteDbPath))
                {
                    db.CreateTable<Song>();
                    db.BeginTransaction();
                    foreach (var item in songs)
                    {
                        item.DownloadState = downloadState;
                        db.InsertOrReplace(item);
                    }
                    db.Commit();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        Songs tmpSongs;
        public void Load()
        {
            List<Song> songs = null;
            try
            {
                string downloadState = System.IO.Path.GetFileNameWithoutExtension(SavePath);
                using (var db = new SQLite.SQLiteConnection(PersistHelper.SqliteDbPath))
                {
                    db.CreateTable<Song>();
                    tmpSongs = PersistHelper.Load<Songs>(SavePath);
                    if (tmpSongs != null && tmpSongs.Count > 0)
                    {
                        db.BeginTransaction();
                        foreach (var item in tmpSongs)
                        {
                            item.DownloadState = downloadState;
                            db.InsertOrReplace(item);
                        }
                        db.Commit();
                    }
                    songs = (from s in db.Table<Song>() where s.DownloadState == downloadState select s).ToList();
                }
                if (songs == null || songs.Count() == 0)
                    return;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            if (Count > 0)
                Clear();
            AddItems(songs);
        }
    }
}
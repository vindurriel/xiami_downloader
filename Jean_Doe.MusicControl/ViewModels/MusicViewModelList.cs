using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jean_Doe.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
namespace Jean_Doe.MusicControl
{
    public class MusicViewModelList : ObservableCollection<MusicViewModel>
    {
        public void AddItems(IEnumerable<IMusic> inlist, bool toFront = false)
        {
            canSave = false;
            int count = inlist.Count();
            if (count == 0) return;
            int s = 1000 / count;
            var list = inlist.ToArray();
            Task.Run(() =>
            {
                foreach (IMusic music in list)
                {
                    addItem(music, toFront);
                    Thread.Sleep(s);
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
        bool canSave = false;
        void addItem(IMusic music, bool toFront)
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
            if (s == null) return;
            if (s is SongViewModel)
            {
                var dup = this.FirstOrDefault(x => x.Id == s.Id) as SongViewModel;
                if (dup != null)
                {
                    return;
                }
                UIHelper.WaitOnUI(() =>
                {
                    if (toFront)
                        Insert(0, s);
                    else
                        Add(s);
                });
                return;
            }
            UIHelper.WaitOnUI(() =>
            {
                Add(s);
            });
        }
        public string SavePath { get; set; }
        public void Save()
        {
            if (!canSave) return;
            var songs = new Songs();
            songs.AddRange(this.OfType<SongViewModel>().Select(x => x.Song));
            PersistHelper.Save(songs, SavePath);
        }
        Songs tmpSongs;
        public async Task Load()
        {
            await Task.Run(() =>
            {
                tmpSongs = PersistHelper.Load<Songs>(SavePath);
            });
            if (tmpSongs == null || tmpSongs.Count == 0)
                return;
            if (Count > 0)
                Clear();
            AddItems(tmpSongs);
        }
    }
}
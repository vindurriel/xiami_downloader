using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jean_Doe.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using music_downloader.Data;
namespace music_downloader
{
    public class MusicViewModelList : ObservableCollection<MusicViewModel>
    {
        public void AddItems(IEnumerable<IMusic> list)
        {
            foreach (IMusic music in list)
            {
                addItem(music);
            }
        }
        void addItem(IMusic music)
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
                case EnumMusicType.song:
                    s = new SongViewModel(music as Song);
                    break;
                default:
                    s = new MusicViewModel(music as Song);
                    break;
            }
            if (s == null) return;
            Add(s);
        }
        public string SavePath { get; set; }
        public void Save()
        {
            var songs = new Songs();
            songs.AddRange(this.OfType<SongViewModel>().Select(x => x.Song));
            //PersistHelper.Save(songs, SavePath);
        }
        public async Task Load()
        {
            Songs songs = null;
            await Task.Run(() =>
            {
                //songs = PersistHelper.Load<Songs>(SavePath);
            });
            if (songs == null)
                return;
            if (Count > 0)
                Clear();
            AddItems(songs);
        }
    }
}
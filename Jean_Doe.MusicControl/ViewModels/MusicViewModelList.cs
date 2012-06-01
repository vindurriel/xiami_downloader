using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jean_Doe.Common;
using System.Linq;
namespace Jean_Doe.MusicControl
{
    public class MusicViewModelList : ObservableCollection<MusicViewModel>
    {
        public void AddItems(IEnumerable<IMusic> list)
        {
            foreach(var music in list)
            {
                MusicViewModel s = null;
                switch(music.Type)
                {
                    case EnumXiamiType.album:
                        s = new AlbumViewModel(music as Album);
                        break;
                    case EnumXiamiType.artist:
                        s = new ArtistViewModel(music as Artist);
                        break;
                    case EnumXiamiType.collect:
                        s = new CollectViewModel(music as Collection);
                        break;
                    case EnumXiamiType.song:
                        s = new SongViewModel(music as Song);
                        break;
                    default:
                        s = new MusicViewModel(music as Song);
                        break;
                }
                if(s != null)
                    Add(s);
            }
        }
        public string SavePath { get; set; }
        public void Save()
        {
            var songs = new Songs();
            songs.AddRange(this.OfType<SongViewModel>().Select(x => x.Song));
            PersistHelper.Save(songs, SavePath);
        }
        public void Load()
        {
            var songs = PersistHelper.Load<Songs>(SavePath);
            if(songs == null) return;
            Clear();
            AddItems(songs);
        }
    }
}
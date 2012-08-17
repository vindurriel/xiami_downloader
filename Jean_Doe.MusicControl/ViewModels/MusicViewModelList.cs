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
        public void AddItems(IEnumerable<IMusic> list,bool now=false)
        {
            if(now)
            {
                foreach(var item in list)
                {
                    addItem(item);
                }
            }
            else {
                Task.Run(new System.Action(() =>
                {
                    var stack = new Stack<IMusic>(list.Reverse());
                    while(stack.Count > 0)
                    {
                        var music = stack.Pop();
                        addItem(music);
                        Thread.Sleep(200);
                    }
                }));
            }
        }
        void addItem(IMusic music)
        {
            MusicViewModel s = null;
            switch(music.Type)
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
            if(s == null) return;

            UIHelper.WaitOnUI(new System.Action(() =>
            {
                Add(s);
            }));
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
            AddItems(songs,now:true);
        }
    }
}
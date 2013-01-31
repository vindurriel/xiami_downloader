

using Metro.IoC;
using music_downloader.Common;
namespace music_downloader.Data
{
    public class Artist : IMusic
    {
        public string Logo
        {
            get;
            set;
        }
        public string Description { get; set; }
        public string Id
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public int AlbumCount { get; set; }
        public EnumMusicType Type
        {
            get { return EnumMusicType.artist; }
        }
        public void CreateFromJson(dynamic obj)
        {
            MusicHelper.LoadMusicInfoFromJson(this, obj);
            AlbumCount = (MusicHelper.Get(obj, "albums_count", "count") as string).ToInt();
        }
    }
}
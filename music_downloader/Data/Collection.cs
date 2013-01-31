using Metro.IoC;
namespace music_downloader.Data
{

    public class Collection : IMusic
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
        public string ArtistId { get; set; }

        public string Name
        {
            get;
            set;
        }
        public EnumMusicType Type
        {
            get { return EnumMusicType.collect; }
        }

        public void CreateFromJson(dynamic obj)
        {
            MusicHelper.LoadMusicInfoFromJson(this, obj);

        }
    }
}
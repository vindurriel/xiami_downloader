using Metro.IoC;
using Windows.Data.Json;

namespace music_downloader.Data
{
    public class MusicFactory
    {
        public static IMusic CreateFromJson(IJsonValue obj, EnumMusicType type)
        {
            IMusic res = null;
            switch(type)
            {
                case EnumMusicType.album:
                    res = new Album();
                    break;
                case EnumMusicType.artist:
                    res = new Artist();
                    break;
                case EnumMusicType.collect:
                    res = new Collection();
                    break;
                case EnumMusicType.song:
                    res = new Song();
                    break;
                default:
                    break;
            }
            if(res != null)
                res.CreateFromJson(obj);
            return res;
        }
    }
}

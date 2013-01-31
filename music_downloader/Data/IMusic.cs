using Metro.IoC;
namespace music_downloader.Data
{
    public interface IMusic
    {
        string Logo { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        EnumMusicType Type { get; }
        void CreateFromJson(dynamic obj);
    }
}
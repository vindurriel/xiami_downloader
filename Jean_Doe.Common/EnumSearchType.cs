using System.ComponentModel;
namespace Jean_Doe.Common
{
    [TypeConverter(typeof(Enum2StringConverter<EnumSearchType>))]
    public enum EnumSearchType
    {
        [Description("所有")]
        all,
        [Description("歌曲")]
        song,
        [Description("艺术家")]
        artist,
        [Description("专辑")]
        album,
        [Description("精选集")]
        collect,
        [Description("相似艺术家")]
        artist_artist,
        [Description("艺术家的歌曲")]
        artist_song,
        [Description("艺术家的专辑")]
        artist_album,
        [Description("专辑的歌曲")]
        album_song,
        [Description("精选集的歌曲")]
        collection_song,
        [Description("我收藏的歌曲")]
        user_song,
        [Description("我收藏的专辑")]
        user_album,
        [Description("我收藏的艺术家")]
        user_artist,
        [Description("我收藏的精选集")]
        user_collect,
    }
}

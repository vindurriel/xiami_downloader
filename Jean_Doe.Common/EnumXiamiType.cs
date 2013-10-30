using System.ComponentModel;
namespace Jean_Doe.Common
{
    [TypeConverter(typeof(Enum2StringConverter<EnumMusicType>))]
    public enum EnumMusicType
    {
        [Description("专辑")]
        album = 1,
        [Description("艺术家")]
        artist = 2,
        [Description("精选集")]
        collect = 3,
        [Description("歌曲")]
        song = 4,
        [Description("所有")]
        all = 0,
    }
}

using Jean_Doe.Common;
using System.Collections.Generic;
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
    public string AlbumCount { get; set; }
    public EnumMusicType Type
    {
        get { return EnumMusicType.artist; }
    }
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);
        Description = MusicHelper.Get(obj, "count_likes");
        if(!string.IsNullOrEmpty(Description))
            Description+="位粉丝";
        AlbumCount = MusicHelper.Get(obj, "albums_count", "count");
        if (!string.IsNullOrEmpty(AlbumCount))
            AlbumCount += "张专辑";
    }
    public dynamic  JsonObject { get; set; }
}

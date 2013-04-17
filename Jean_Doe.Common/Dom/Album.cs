using System.Collections.Generic;
using Jean_Doe.Common;
public class Album : IMusic
{
    public string Logo
    {
        get;
        set;
    }

    public string Id
    {
        get;
        set;
    }
    public Dictionary<string, object> JsonObject { get; set; }
    public string ArtistId { get; set; }
    public string ArtistName { get; set; }
    public string Name
    {
        get;
        set;
    }
    public EnumMusicType Type
    {
        get { return EnumMusicType.album; }
    }
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);
        ArtistId = MusicHelper.Get(obj, "artist_id");
        ArtistName = MusicHelper.Get(obj, "artist_name");
    }
}

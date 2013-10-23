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
    public dynamic  JsonObject { get; set; }
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
        if (string.IsNullOrEmpty(ArtistId) && obj["songs"]!=null)
        {
            ArtistId = MusicHelper.Get(obj["songs"][0], "artist_id");
        }
    }
}

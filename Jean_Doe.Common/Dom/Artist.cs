using Jean_Doe.Common;
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
    public EnumXiamiType Type
    {
        get { return EnumXiamiType.artist; }
    }
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);
        AlbumCount = (MusicHelper.Get(obj, "albums", "count") as string).ToInt();
    }
}

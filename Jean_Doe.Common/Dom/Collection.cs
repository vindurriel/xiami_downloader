using Jean_Doe.Common;
public class Collection : IMusic
{
    string logo;
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
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);

    }
	public string Type
	{
		get { return "collect"; }
	}
}

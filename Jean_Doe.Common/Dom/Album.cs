using System.Collections.Generic;
using Jean_Doe.Common;
public class Album : IMusic
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
    public List<Song> Songs = new List<Song>();
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);
        ArtistId = MusicHelper.Get(obj, "artist_id");
        ArtistName = MusicHelper.Get(obj, "artist_name");
        if(obj.ContainsKey("songs"))
        if (obj.songs != null)
        {
            foreach (var item in obj.songs)
            {
                var song = MusicFactory.CreateFromJson(obj, EnumMusicType.song);
                if (song != null) Songs.Add(song);
            }
        }
    }
}

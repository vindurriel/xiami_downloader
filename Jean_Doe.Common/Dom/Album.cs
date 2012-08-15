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
    public EnumXiamiType Type
    {
        get { return EnumXiamiType.album; }
    }
    public List<Song> Songs = new List<Song>();
    public void CreateFromJson(dynamic obj)
    {
        MusicHelper.LoadMusicInfoFromJson(this, obj);
        ArtistId = MusicHelper.Get(obj, "artist", "id");
        ArtistName = MusicHelper.Get(obj, "artist", "name");
        if(obj.ContainsKey("songs"))
        if (obj.songs != null)
        {
            foreach (var item in obj.songs)
            {
                var song = MusicFactory.CreateFromJson(obj, EnumXiamiType.song);
                if (song != null) Songs.Add(song);
            }
        }
    }
}

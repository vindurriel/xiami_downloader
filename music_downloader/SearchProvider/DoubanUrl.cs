using System.Collections.Generic;
public static class DoubanUrl
{
    static readonly string lyric = "http://site.douban.com/widget/playlist/{0}/get_song_lyrics?sid={1}";
    public static string LyricUrl(string listId, string songId)
    {
        return string.Format(lyric, listId, songId);
    }
}
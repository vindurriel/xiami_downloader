using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jean_Doe.Common;
using System;
public class BaiduSearchProvider : ISearchProvider
{
    static readonly Regex re_url = new Regex(@"music\.baidu\.com");
    static readonly Regex re_download = new Regex("link=(http://zhangmenshiting.baidu.com/data2/music/[^\"]+)");
    static readonly Regex re_artist = new Regex("\"site_title\":\\s*\"([^\"]+?)\"");
    static readonly string search_url = "http://openapi.baidu.com/public/2.0/mp3/info/suggestion?format=json&word={0}";
    static readonly string download_url = "http://ting.baidu.com/song/%s/download";
    public async Task<SearchResult> Search(string key, EnumSearchType t)
    {
        key = key.Trim();
        var res = new SearchResult
        {
            Items = new List<IMusic>(),
            Keyword = key,
            SearchType = t,
            Page = 1,
        };
        var url = string.Format(search_url, Uri.EscapeDataString(key));
        var response = await NetAccess.DownloadStringAsync(url);
        var json = response.ToDynamicObject();
        if (json.song.Count > 0)
            foreach (var obj in json.song)
            {
                var s = new Song
                {
                    Id = "b" + MusicHelper.Get(obj, "songid"),
                    ArtistName = MusicHelper.Get(obj, "artistname"),
                    AlbumName = "",
                    Name = MusicHelper.Get(obj, "songname"),
                    WriteId3 = false,
                };
                res.Items.Add(s);
                s.UrlMp3 =await getDownloadUrl(s.Id.Substring(1));
            }
        return res;
    }
    async Task<string> getDownloadUrl(string id)
    {
        var url=string.Format("http://music.baidu.com/song/{0}/download",id);
       var html= await NetAccess.DownloadStringAsync(url);
        var m=re_download.Match(html);
        if(!m.Success)
            return null;
        return m.Groups[1].Value;
    }

    public Regex Pattern
    {
        get { return re_url; }
    }
}
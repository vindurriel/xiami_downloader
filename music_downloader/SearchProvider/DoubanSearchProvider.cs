﻿using music_downloader.Common;
using music_downloader.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
public class DoubanSearchProvider : ISearchProvider
{
    static readonly Regex re_url = new Regex(@"site\.douban\.com");
    static readonly Regex re_json = new Regex(@"PlaylistWidget\.findOrCreate\((\d+)\),\n.*?song_records\s*=([^;]+?);");
    static readonly Regex re_artist = new Regex("\"site_title\":\\s*\"([^\"]+?)\"");

    public  Regex Pattern
    {
        get { return new Regex(@"douban\.(?:com|fm)"); }
    }
    public async Task<SearchResult> Search(string key)
    {
        var res = new SearchResult
        {
            Items = new List<IMusic>(),
            Keyword = key,
            SearchType = EnumSearchType.url,
            Page = 1,
        };
        if (string.IsNullOrEmpty(key) || !re_url.IsMatch(key)) return res;
        var html = await NetAccess.DownloadStringAsync(key);
        var ms = re_json.Matches(html);
        if (ms.Count == 0) return null;
        var artistName = "";
        var x=re_artist.Match(html);
        if (x.Success)
            artistName=x.Groups[1].Value;
        foreach (Match m in ms)
        {
            var listId = m.Groups[1].Value;
            var json = JsonParser.Parse(m.Groups[2].Value);
            foreach (var obj in json.GetArray())
            {
                var s = new Song
                {
                    Id = "d"+MusicHelper.Get(obj, "id"),
					AlbumName=artistName,
                    ArtistName=artistName,
                    Name = MusicHelper.Get(obj, "name"),
                    UrlMp3 = (MusicHelper.Get(obj, "rawUrl") as string).Replace("\\/","/"),
                    Logo = MusicHelper.Get(obj, "cover"),
					WriteId3=false,
                };
				s.UrlLrc=DoubanUrl.LyricUrl(listId, s.Id.Substring(1));
				
                res.Items.Add(s);
            }
        }
        return res;
    }
}
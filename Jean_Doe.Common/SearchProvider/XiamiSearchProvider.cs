using System.Threading.Tasks;
using Jean_Doe.Common;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Text.RegularExpressions;
public class XiamiSearchProvider : ISearchProvider
{
    public Regex Pattern
    {
        get { return new Regex(@"xiami\.com"); }
    }
    public async Task<SearchResult> Search(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var m = Regex.Match(key, "user:(\\d+)");
        if (m.Success)
        {
            await GetUserSongs(m.Groups[1].Value);
            return null;
        }
        if (Uri.IsWellFormedUriString(key, UriKind.Absolute))
            return await SearchByUrl(key);
        else
            return await SearchByKey(key);
    }
    static async Task<SearchResult> SearchByType(EnumSearchType type, string id)
    {
        string url = "";
        if (type == EnumSearchType.artist || type == EnumSearchType.artist_song)
            url = XiamiUrl.UrlArtistTopSong(id);
        else if (type == EnumSearchType.artist_similar)
            url = XiamiUrl.UrlArtistSimilars(id);
        else if (type == EnumSearchType.artist_album)
            url = XiamiUrl.UrlArtistAlbums(id);
        else
            url = XiamiUrl.UrlPlaylistByIdAndType(id, type.ToString());
        var json = await NetAccess.DownloadStringAsync(url);
        ///////////////////////////////////////////////////////
        if (json == null) return null;
        List<IMusic> items = new List<IMusic>();
        var searchType = EnumSearchType.song;
        var newKey = "";
        switch (type)
        {
            case EnumSearchType.song:
                searchType = EnumSearchType.song;
                items = GetSong(json, out newKey);
                break;
            case EnumSearchType.album:
            case EnumSearchType.album_song:
                searchType = EnumSearchType.album_song;
                items = GetSongsOfAlbum(json, out newKey);
                break;
            case EnumSearchType.artist:
            case EnumSearchType.artist_song:
                searchType = EnumSearchType.artist_song;
                items = GetSongsOfArtist(json, out newKey);
                break;
            case EnumSearchType.collect:
            case EnumSearchType.collection_song:
                searchType = EnumSearchType.collection_song;
                items = GetSongsOfCollect(json, out newKey);
                break;
            case EnumSearchType.artist_similar:
                searchType = EnumSearchType.artist_similar;
                items = GetSimilarsOfArtist(json, out newKey);
                break;
            case EnumSearchType.artist_album:
                searchType = EnumSearchType.artist_album;
                items = GetAlbumsOfArtist(json, out newKey);
                break;
            default:
                break;
        }
        var res = new SearchResult
        {
            Items = items,
            Keyword = string.IsNullOrEmpty(newKey) ? id : newKey,
            SearchType = searchType,
            Page = 1,
        };
        return res;
    }

    static async Task<SearchResult> SearchAll(string key)
    {
        string url = XiamiUrl.UrlSearchAll(key);
        string json = await NetAccess.DownloadStringAsync(url);
        /////////////
        dynamic obj = json.ToDynamicObject();
        var items = new List<IMusic>();
        foreach (var type in new string[] { "song", "album", "artist", "collect" })
        {
            var data = obj[type + "s"] as ArrayList;
            if (data == null) continue;
            foreach (dynamic x in data)
            {
                items.Add(MusicFactory.CreateFromJson(x, (EnumMusicType)Enum.Parse(typeof(EnumMusicType), type)));
            }
        }
        var sr = new SearchResult
        {
            Items = items,
            Keyword = key,
            Page = -1,
            SearchType = EnumSearchType.all,
        };
        return sr;
    }
    public static async Task GetUserSongs(string userId)
    {
        int page = 1;
        while (true)
        {
            var url = string.Format("/app/iphone/lib-songs/uid/{0}/page/{1}", userId, page);
            var json = await XiamiClient.GetDefault().GetString(url);
            if (json == null) break;
            dynamic obj = json.ToDynamicObject();
            if (obj == null) break;
            var items = new List<IMusic>();
            foreach (dynamic item in obj.songs)
            {
                try
                {
                    items.Add(MusicFactory.CreateFromJson(item, EnumMusicType.song));
                }
                catch (Exception e)
                {
                }
            }
            var sr = new SearchResult { Items = items, SearchType = EnumSearchType.user_song };
            if (sr == null || sr.Count == 0 || SearchManager.State == EnumSearchState.Cancelling)
            {
                break;
            }
            SearchManager.notifyState(sr);
            page++;
            if (obj.more != "true")
                break;
        }
    }

    static async Task<SearchResult> SearchByKey(string key)
    {

        EnumMusicType type = EnumMusicType.song;
        Enum.TryParse(Global.AppSettings["SearchResultType"], out type);
        if (type == EnumMusicType.any)
        {
            return await SearchAll(key);
        }
        int page = 1;
        while (true)
        {
            SearchResult sr = await _searchByKey(key, page, type);
            /////////////////////////////////////////
            if (sr == null || sr.Count == 0 || SearchManager.State == EnumSearchState.Cancelling)
            {
                break;
            }

            SearchManager.notifyState(sr);
            page++;
        }
        return null;
    }
    async static Task<SearchResult> _searchByKey(string keyword, int page, EnumMusicType type = EnumMusicType.song)
    {
        string url = XiamiUrl.UrlSearch(keyword, page, type);
        string json = await NetAccess.DownloadStringAsync(url);
        /////////////
        if (json == null) return null;
        dynamic obj = json.ToDynamicObject();
        if (obj == null) return null;
        var data = obj.data as IList<dynamic>;
        if (data == null) return null;
        var items = new List<IMusic>();
        foreach (dynamic x in data)
        {
            items.Add(MusicFactory.CreateFromJson(x, type));
        }
        var searchType = EnumSearchType.song;
        Enum.TryParse<EnumSearchType>(type.ToString(), out searchType);
        var res = new SearchResult
        {
            Items = items,
            Keyword = keyword,
            Page = page,
            SearchType = searchType,
        };
        return res;
    }
    static async Task<SearchResult> SearchByUrl(string url)
    {
        var patterns = new List<Regex>{
                new Regex(@"(album|artist|song)/(\d+)"),//album,artist,song
                new Regex(@"show(collect)/id/(\d+)"),//collection
                new Regex(@"type/([^/]+?)/id/(\d+)"),//for fake urls to search by type
            };
        string strType = null, id = null;
        bool IsPatternRecognized = false;
        foreach (var pattern in patterns)
        {
            var j = pattern.Match(url);
            if (j.Success)
            {
                strType = j.Groups[1].Value;
                id = j.Groups[2].Value;
                IsPatternRecognized = true;
                break;
            }
        }
        if (!IsPatternRecognized)
            return null;
        var t = EnumSearchType.song;
        Enum.TryParse<EnumSearchType>(strType, out t);
        var res = await SearchByType(t, id);
        return res;
    }
    static List<IMusic> GetSimilarsOfArtist(string json, out string musicName)
    {
        var items = new List<IMusic>();
        musicName = "";
        try
        {
            var obj = json.ToDynamicObject().artists;
            foreach (var x in obj)
            {
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.artist));
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetAlbumsOfArtist(string json, out string musicName)
    {
        var items = new List<IMusic>();
        musicName = "";
        try
        {
            var obj = json.ToDynamicObject().albums;
            foreach (var x in obj)
            {
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.album));
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSongsOfArtist(string json, out string musicName)
    {
        var items = new List<IMusic>();
        musicName = "";
        try
        {
            var obj = json.ToDynamicObject().songs;
            foreach (var x in obj)
            {
                if (musicName == "")
                    musicName = x.artist_name;
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.song));
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSongsOfCollect(string json, out string musicName)
    {
        var items = new List<IMusic>();
        try
        {
            var obj = json.ToDynamicObject().collect;
            musicName = obj.name;
            foreach (var x in obj.songs)
            {
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.song));
            }
        }
        catch { musicName = ""; }
        return items;
    }
    static List<IMusic> GetSongsOfAlbum(string json, out string musicName)
    {
        var items = new List<IMusic>();
        musicName = "";
        try
        {
            dynamic obj = json.ToDynamicObject();
            var album_name = obj.album.title;
            if (musicName == "")
                musicName = album_name;
            foreach (var x in obj.album.songs)
            {
                Song a = MusicFactory.CreateFromJson(x, EnumMusicType.song);
                a.AlbumName = album_name;
                items.Add(a);
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSong(string json, out string musicName)
    {
        List<IMusic> res = new List<IMusic>();
        musicName = "";
        try
        {
            var obj = json.ToDynamicObject().song;
            var song = MusicFactory.CreateFromJson(obj, EnumMusicType.song);
            musicName = song.Name;
            res.Add(song);
        }
        catch { }
        return res;
    }
    //public async static Task<Album> GetAlbum(string id)
    //{
    //    var url = XiamiUrl.UrlPlaylistByIdAndType(id, EnumMusicType.album.ToString());
    //    var json = await NetAccess.DownloadStringAsync(url);
    //    if(json == null) return null;
    //    var obj = json.ToDynamicObject().album;
    //    var Album = MusicFactory.CreateFromJson(obj, EnumMusicType.album);
    //    return Album;
    //}
}
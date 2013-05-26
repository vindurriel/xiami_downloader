using System.Threading.Tasks;
using Jean_Doe.Common;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CsQuery;
using System.IO;
public class XiamiSearchProvider : ISearchProvider
{
    public Regex Pattern
    {
        get { return new Regex(@"xiami\.com"); }
    }
    public async Task<SearchResult> Search(string key, EnumSearchType t)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var m = Regex.Match(key, "user:(\\w+)");
        if (m.Success)
        {
            await getUserMusic(m.Groups[1].Value, t.ToString());
            return null;
        }
        m = Regex.Match(key, "(artist|album|collect):(\\d+)");
        if (m.Success)
        {
            var str_type = m.Groups[1].Value + "_" + t.ToString();
            Enum.TryParse(str_type, out t);
            return await getByType(t, m.Groups[2].Value);
        }
        if (Uri.IsWellFormedUriString(key, UriKind.Absolute))
            return await SearchByUrl(key);
        var musicType = EnumMusicType.all;
        Enum.TryParse(t.ToString(), out musicType);
        return await SearchByKey(key, musicType);
    }
    static void getCollectDetails(string id)
    {
        Task.Run(() =>
        {
            string url = XiamiUrl.GoCollect(id);
            var tmp = Path.Combine(Global.BasePath, string.Format("collect.{0}.html", id));
            if (!File.Exists(tmp))
            {
                new System.Net.WebClient().DownloadFile(url, tmp);
            }
            var doc = CQ.CreateFromFile(tmp);
            foreach (var x in CQ.Create(doc["li[class^=\"totle_\"]"]))
            {
                var item = CQ.Create(x);
                var desc = item[".s_quote strong"];
                if (desc.Length == 0) continue;
                var song_id = x.Id.Substring(6);
                var description = desc.Text();
                UIHelper.RunOnUI(() =>
                {
                    Artwork.MessageBus.MessageBus.Instance.Publish(
                        new MsgSetDescription { Id = song_id, Description = description });
                });


            }
        });
    }
    static async Task<SearchResult> getByType(EnumSearchType type, string id)
    {
        List<IMusic> items = new List<IMusic>();
        dynamic json = null;
        switch (type)
        {
            case EnumSearchType.song:
                json = await XiamiClient.GetDefault().Call_xiami_api("Songs.detail", "id=" + id);
                items = GetSong(json);
                break;
            case EnumSearchType.album_song:
                json = await XiamiClient.GetDefault().Call_xiami_api("Albums.detail", "id=" + id);
                items = GetSongsOfAlbum(json);
                break;
            case EnumSearchType.artist_song:
                json = await XiamiClient.GetDefault().Call_xiami_api("Artists.hotSongs", "id=" + id);
                items = GetSongsOfArtist(json);
                break;
            case EnumSearchType.collect_song:
                json = await XiamiClient.GetDefault().Call_xiami_api("Collects.detail", "id=" + id);
                getCollectDetails(json.list_id);
                items = GetSongsOfCollect(json);
                break;
            case EnumSearchType.artist_artist:
                json = await NetAccess.DownloadStringAsync(XiamiUrl.UrlArtistSimilars(id));
                items = GetSimilarsOfArtist(json);
                break;
            case EnumSearchType.artist_album:
                json = await XiamiClient.GetDefault().Call_xiami_api("Artists.albums", "id=" + id);
                items = GetAlbumsOfArtist(json);
                break;
            default:
                break;
        }
        if (json == null) return null;
        if (type.ToString().Contains("_"))
        {
            var duo = type.ToString().Split("_".ToCharArray());
            id = duo[0] + ":" + id;
            Enum.TryParse(duo[1], out type);
        }
        var res = new SearchResult
        {
            Items = items,
            Keyword = id,
            SearchType = type,
            Page = 1,
        };
        return res;
    }

    static async Task<SearchResult> SearchAll(string key)
    {
        dynamic obj = await XiamiClient.GetDefault().Call_xiami_api("Search.summary", string.Format("\"key={0}\"", key));
        /////////////
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
    static async Task getUserMusic(string key, string t)
    {
        if (t == "all" || t == "any") t = "song";
        var searchType = EnumSearchType.song;
        Enum.TryParse(t.ToString(), out searchType);
        var musicType = EnumMusicType.song;
        Enum.TryParse(t.ToString(), out musicType);
        int page = 1;
        if (key == "me")
            key = "lib";
        while (true)
        {
            dynamic obj = null;
            if (key == "daily")
                obj = await XiamiClient.GetDefault().Call_xiami_api("Recommend.DailySongs");
            else if (key == "guess")
                obj = await XiamiClient.GetDefault().GetGuess();
            else if (key == "lib")
                obj = await XiamiClient.GetDefault().GetUserMusic(t, page);
            else if (key == "collect_recommend")
                obj = await XiamiClient.GetDefault().Call_xiami_api("Collects.recommend");
            else
                throw new Exception("user:" + key + " is not supported");
            if (obj == null) break;
            var items = new List<IMusic>();
            var list = obj[musicType.ToString() + "s"];
            if (list == null) break;
            foreach (dynamic item in list)
            {
                try
                {
                    items.Add(MusicFactory.CreateFromJson(item, musicType));
                }
                catch (Exception e)
                {
                }
            }
            var sr = new SearchResult { Keyword = "user:" + key, Items = items, SearchType = searchType };
            if (sr == null || sr.Count == 0 || SearchManager.State == EnumSearchState.Cancelling)
            {
                break;
            }
            Artwork.MessageBus.MessageBus.Instance.Publish(sr);
            if (key == "lib" && t == "song")
            {
                foreach (Song item in sr.Items)
                {
                    item.InFav = true;
                }
            }
            SearchManager.notifyState(sr);
            if (obj.more != "true")
                break;
            page++;
        }
    }

    static async Task<SearchResult> SearchByKey(string key, EnumMusicType type)
    {
        if (type == EnumMusicType.all)
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
            Artwork.MessageBus.MessageBus.Instance.Publish(sr);
            SearchManager.notifyState(sr);
            if (!sr.HasNext) break;
            page++;
        }
        return null;
    }
    async static Task<SearchResult> _searchByKey(string keyword, int page, EnumMusicType type = EnumMusicType.song)
    {
        //string url = XiamiUrl.UrlSearch(keyword, page, type);
        //string json = await NetAccess.DownloadStringAsync(url);
        ///////////////
        //if (json == null) return null;
        string typestr = type.ToString();
        dynamic obj = await XiamiClient.GetDefault().Call_xiami_api(string.Format("Search.{0}s", typestr),
            string.Format("\"key={0}\"", keyword),
            string.Format("page={0}", page));
        if (obj == null) return null;
        var data = obj[typestr + "s"] as ArrayList;
        if (data == null) return null;
        var items = new List<IMusic>();
        foreach (dynamic x in data)
        {
            items.Add(MusicFactory.CreateFromJson(x, type));
        }
        var searchType = EnumSearchType.song;
        Enum.TryParse<EnumSearchType>(type.ToString(), out searchType);
        bool hasNext = page != (int)obj.next;
        var res = new SearchResult
        {
            Items = items,
            Keyword = keyword,
            Page = page,
            HasNext = hasNext,
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
        var res = await getByType(t, id);
        res.Keyword = url;
        return res;
    }
    static List<IMusic> GetSimilarsOfArtist(string json)
    {
        var items = new List<IMusic>();
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
    static List<IMusic> GetAlbumsOfArtist(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            foreach (var x in json.albums)
            {
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.album));
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSongsOfArtist(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            foreach (var x in json.songs)
            {
                items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.song));
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSongsOfCollect(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            foreach (var x in json.songs)
            {
                string id = x.song_id;
                Song song = MusicFactory.CreateFromJson(x, EnumMusicType.song);
                items.Add(song);
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSongsOfAlbum(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            dynamic obj = json;
            foreach (var x in obj.songs)
            {
                Song a = MusicFactory.CreateFromJson(x, EnumMusicType.song);
                items.Add(a);
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetSong(dynamic json)
    {
        List<IMusic> res = new List<IMusic>();
        try
        {
            var obj = json.song;
            var song = MusicFactory.CreateFromJson(obj, EnumMusicType.song);
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
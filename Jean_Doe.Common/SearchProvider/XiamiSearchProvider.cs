using System.Threading.Tasks;
using Jean_Doe.Common;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
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
            await getUserMusic(m.Groups[1].Value);
            return null;
        }
        m = Regex.Match(key, "(song|artist|album|collect):(\\d+)");
        if (m.Success)
        {
            var str_type = m.Groups[1].Value + "_" + t.ToString();
            if (!Enum.TryParse(str_type, out t))
                Enum.TryParse(m.Groups[1].Value, out t);
            return await getByType(t, m.Groups[2].Value);
        }
        if (Uri.IsWellFormedUriString(key, UriKind.Absolute))
            return await SearchByUrl(key);
        var musicType = EnumMusicType.all;
        Enum.TryParse(t.ToString(), out musicType);
        return await SearchAll (key);
    }
    static void getCollectDetails(string id)
    {
        Task.Run(() =>
        {
            string url = XiamiUrl.GoCollect(id);
            var tmp = Path.Combine(Global.BasePath, "cache", string.Format("collect.{0}.html", id));
            if (!File.Exists(tmp))
            {
                new System.Net.WebClient().DownloadFile(url, tmp);
            }
            var doc = new HtmlDocument();
            doc.Load(tmp, System.Text.Encoding.UTF8);
            foreach (var x in doc.DocumentNode.SelectNodes("//li[@class='totle_up']"))
            {
                var desc = x.SelectSingleNode(".//strong");
                if (desc == null) continue;
                var song_id = x.Id.Substring(6);
                var description = desc.InnerText;
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
                json = await NetAccess.Json(XiamiUrl.url_song,"id",id);
                items = GetSong(json);
                break;
            case EnumSearchType.album:
                json = await NetAccess.Json(XiamiUrl.url_album, "id", id);
                items = GetAlbum(json);
                break;
            case EnumSearchType.album_song:
                json = await NetAccess.Json(XiamiUrl.url_album, "id", id);
                items = GetSongsOfAlbum(json);
                break;
            case EnumSearchType.artist_song:
                json = await NetAccess.Json(XiamiUrl.url_artist_top_song, "id", id);
                items = GetSongsOfArtist(json);
                break;
            case EnumSearchType.artist:
                json = await NetAccess.Json(XiamiUrl.url_artist, "id", id);
                items = GetArtist(json);
                break;
            case EnumSearchType.collect:
            case EnumSearchType.collect_song:
                json = await NetAccess.Json(XiamiUrl.url_collect, "id", id);
                getCollectDetails(json["list_id"]);
                items = GetSongsOfCollect(json);
                break;
            case EnumSearchType.artist_artist:
                json = await NetAccess.Json(XiamiUrl.url_artsit_similars, "id", id);
                items = GetSimilarsOfArtist(json);
                break;
            case EnumSearchType.artist_album:
                json = await NetAccess.Json(XiamiUrl.url_artist_albums, "id", id);
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
        else
        {
            id = type.ToString() + ":" + id;
            type = EnumSearchType.all;
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
        var items = new List<IMusic>();
        var sr = new SearchResult
        {
            Items = items,
            Keyword = key,
            Page = -1,
            SearchType = EnumSearchType.all,
        };
        dynamic obj = await NetAccess.Json(XiamiUrl.url_search_all, "key", key);
        if (obj == null) return sr;
        foreach (var type in new string[] { "song", "album", "artist", "collect" })
        {
            var data = obj[type + "s"] as ArrayList;
            if (data == null) continue;
            foreach (dynamic x in data)
            {
                items.Add(MusicFactory.CreateFromJson(x, (EnumMusicType)Enum.Parse(typeof(EnumMusicType), type)));
            }
        }
        return sr;
    }
    static async Task getUserMusic(string key)
    {
        bool isKeyValidMusicType = new Regex("^(song|album|artist|collect)$").IsMatch(key.Trim());
        var searchType = EnumSearchType.song;
        var musicType = EnumMusicType.song;
        if (isKeyValidMusicType)
        {
            Enum.TryParse(key, out searchType);
            Enum.TryParse(key, out musicType);
        }
        int page = 1;
        while (true)
        {
            dynamic obj = null;
            if (key == "daily")
                obj = await XiamiClient.GetDefault().Call_xiami_api("Recommend.DailySongs");
            else if (key == "guess")
                obj = await XiamiClient.GetDefault().GetGuess();
            else if (key == "collect_recommend")
            {
                musicType = EnumMusicType.collect;
                searchType = EnumSearchType.collect;
                obj = await XiamiClient.GetDefault().Call_xiami_api("Collects.recommend");
            }
            else if (isKeyValidMusicType)
                obj = await XiamiClient.GetDefault().GetUserMusic(key, page);
            else
            {
                Logger.Error(new Exception("user:" + key + " is not supported"));
                break;
            }
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
            if (key == "song")
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
        dynamic obj =await  NetAccess.Json(XiamiUrl.url_search_all, "key", keyword, "page", page.ToString());
        /////////////
        if (obj == null) return null;
        //string typestr = type.ToString();
        //dynamic obj = await XiamiClient.GetDefault().Call_xiami_api(string.Format("Search.{0}s", typestr),
        //    string.Format("\"key={0}\"", keyword),
        //    string.Format("page={0}", page));
        //if (obj == null) return null;
        var data = obj[type.ToString() + "s"] as ArrayList;
        if (data == null) return null;
        var items = new List<IMusic>();
        foreach (dynamic x in data)
        {
            items.Add(MusicFactory.CreateFromJson(x, type));
        }
        var searchType = EnumSearchType.song;
        Enum.TryParse<EnumSearchType>(type.ToString(), out searchType);
        bool hasNext =  page.ToString() !=  obj["next"];
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
    static List<IMusic> GetArtist(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            items.Add(MusicFactory.CreateFromJson(json, EnumMusicType.artist));
        }
        catch { }
        return items;
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
            foreach (var x in json["albums"])
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
            foreach (var x in json["songs"])
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
            foreach (var x in json["songs"])
            {
                string id = x["song_id"];
                Song song = MusicFactory.CreateFromJson(x, EnumMusicType.song);
                items.Add(song);
            }
        }
        catch { }
        return items;
    }
    static List<IMusic> GetAlbum(dynamic json)
    {
        var items = new List<IMusic>();
        try
        {
            items.Add(MusicFactory.CreateFromJson(json, EnumMusicType.album));
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
            foreach (var x in obj["songs"])
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
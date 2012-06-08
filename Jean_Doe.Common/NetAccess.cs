using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using Artwork.MessageBus;
using System.Windows;
using System.Web.Script.Serialization;
using System.Collections;
namespace Jean_Doe.Common
{
    public static class NetAccess
    {
        public static void CancelAsync()
        {
            if(client != null && client.IsBusy)
            {
                client.CancelAsync();
            }
        }
        static WebClient client;
        public async static Task<string> DownloadStringAsync(string url)
        {
            client = new WebClient();
            string res = null;
            try
            {
                res = await client.DownloadStringTaskAsync(url);
            }
            catch { }
            finally
            {
                client.Dispose();
            }
            return res;
        }

        public async static Task<string> GetUrlLrc(string songId)
        {
            var json = await NetAccess.DownloadStringAsync(XiamiUrl.UrlSong(songId));
            try
            {
                return json.ToDynamicObject().song.song_lrc as string;
            }
            catch
            {
                return null;
            }
        }
        public async static Task<SearchResult> Search(string keyword, int page, EnumXiamiType type = EnumXiamiType.song)
        {
            string url = XiamiUrl.UrlSearch(keyword, page, type);
            string json = await DownloadStringAsync(url);
            /////////////
            dynamic obj = json.ToDynamicObject();
            if(obj == null) return null;
            var data = obj.data as IList<dynamic>;
            if(data == null) return null;
            var items = new List<IMusic>();
            foreach(dynamic x in data)
            {
                items.Add(MusicFactory.CreateFromJson(x, type));
            }
            var res = new SearchResult
           {
               Items = items,
               Keyword = keyword,
               Page = page,
               SearchType = EnumSearchType.key
           };
            return res;
        }
        public async static Task<SearchResult> SearchByUrl(string url)
        {
            var patterns = new List<Regex>{
                new Regex(@"(album|artist|song)/(\d+)"),//album,artist,song
                new Regex(@"show(collect)/id/(\d+)"),//collection
            };
            string strType = null, id = null;
            bool IsPatternRecognized = false;
            foreach(var pattern in patterns)
            {
                var j = pattern.Match(url);
                if(j.Success)
                {
                    strType = j.Groups[1].Value;
                    id = j.Groups[2].Value;
                    IsPatternRecognized = true;
                    break;
                }
            }
            if(!IsPatternRecognized)
                return null;
            EnumXiamiType type = EnumXiamiType.song;
            Enum.TryParse<EnumXiamiType>(strType, out type);
            var res = await GetSongsOfType(id, type);
            res.SearchType = EnumSearchType.url;
            res.Keyword = url;
            return res;
        }
        public async static Task<SearchResult> GetSongsOfType(string id, EnumXiamiType type)
        {
            string url = XiamiUrl.UrlPlaylistByIdAndType(id, type);
            if(type == EnumXiamiType.artist)
            {
                url = XiamiUrl.UrlArtistTopSong(id);
            }
            var json = await DownloadStringAsync(url);
            ///////////////////////////////////////////////////////
            List<IMusic> items = new List<IMusic>();
            switch(type)
            {
                case EnumXiamiType.album:
                    items = GetSongsOfAlbum(json);
                    break;
                case EnumXiamiType.artist:
                    items = GetSongsOfArtist(json);
                    break;
                case EnumXiamiType.collect:
                    items = GetSongsOfCollect(json);
                    break;
                case EnumXiamiType.song:
                    var song = await GetSong(id);
                    if(song != null)
                        items.Add(song);
                    break;
                case EnumXiamiType.any:
                    break;
                default:
                    break;
            }
            var res = new SearchResult
            {
                Items = items,
                Keyword = id,
                SearchType = EnumSearchType.type,
                Page=1,
            };
            return res;
        }
         static List<IMusic> GetSongsOfArtist(string json)
        {
            var items = new List<IMusic>();
            try
            {
                var obj = json.ToDynamicObject().songs;
                foreach(var x in obj)
                {
                    items.Add(MusicFactory.CreateFromJson(x, EnumXiamiType.song));
                }
            }
            catch { }
            {
            }
            return items;
        }
         static List<IMusic> GetSongsOfCollect(string json)
        {
            var items = new List<IMusic>();
            try
            {
                var obj = json.ToDynamicObject().collect.songs;
                foreach(var x in obj)
                {
                    items.Add(MusicFactory.CreateFromJson(x, EnumXiamiType.song));
                }
            }
            catch { }
            {
            }
            return items;
        }
         static List<IMusic> GetSongsOfAlbum(string json)
        {
            var items = new List<IMusic>();
            try
            {
                dynamic obj = json.ToDynamicObject();
                var album_name = obj.album.title;
                foreach(var x in obj.album.songs)
                {
                    Song a = MusicFactory.CreateFromJson(x, EnumXiamiType.song);
                    a.AlbumName = album_name;
                    items.Add(a);
                }
            }
            catch { }
            return items;
        }
         async static Task<Song> GetSong(string id)
        {
            var url = XiamiUrl.UrlPlaylistByIdAndType(id, EnumXiamiType.song);
            Song song = null;
            try
            {
                var json = await DownloadStringAsync(url);
                ////////
                var obj = json.ToDynamicObject().song;
                song = MusicFactory.CreateFromJson(obj, EnumXiamiType.song);
            }
            catch { }
            return song;
        }
        public async static Task<Album> GetAlbum(string id)
        {
            var url = XiamiUrl.UrlPlaylistByIdAndType(id, EnumXiamiType.album);
            var json = await DownloadStringAsync(url);
            if(json == null) return null;
            var obj = json.ToDynamicObject().album;
            var Album = MusicFactory.CreateFromJson(obj, EnumXiamiType.album);
            return Album;
        }
        public static dynamic ToDynamicObject(this string json)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            try
            {
                return serializer.Deserialize(json, typeof(object)) as dynamic;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}

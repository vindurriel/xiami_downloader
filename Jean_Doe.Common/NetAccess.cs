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
using System.Net.Http;
namespace Jean_Doe.Common
{
    public static class NetAccess
    {
        static object lck = new object();
        public static void CancelAsync()
        {
            Task.Run(() =>
            {
                lock (lck)
                {
                    foreach (var token in cancelTokens)
                    {
                        if (token == null || token.IsCancellationRequested) continue;
                        token.Cancel(false);
                    }
                }
            });
        }
        static List<CancellationTokenSource> cancelTokens = new List<CancellationTokenSource>();
        static CancellationTokenSource cancelToken = new CancellationTokenSource();
        public async static Task<string> DownloadStringAsync(string url)
        {
            var tcs = new CancellationTokenSource();
            var client = new HttpClient();
            lock (lck)
            {
                cancelTokens.Add(tcs);
            }
            string res = null;
            try
            {
                var x = await client.GetAsync(url, tcs.Token);
                res = await x.Content.ReadAsStringAsync();
                if (tcs.IsCancellationRequested)
                    throw new OperationCanceledException("");
            }
            catch (Exception e)
            {
               
            }
            finally
            {
                client.Dispose();
                lock (lck)
                {
                    cancelTokens.Remove(tcs);
                }
                tcs.Dispose();
            }
            return res;
        }

        public async static Task<string> GetUrlLrc(string songId)
        {
            var json = await NetAccess.DownloadStringAsync(XiamiUrl.UrlSong(songId));
            if (json == null) return null;
            try
            {
                return json.ToDynamicObject().lyric as string;
            }
            catch
            {
                return null;
            }
        }
        public async static Task<double> GetPlayTimes(string songId)
        {
            double res = 0;
            var json = await NetAccess.DownloadStringAsync(XiamiUrl.UrlSong(songId));
            if (json == null) return 0;
            try
            {
                var str = json.ToDynamicObject().year_play as string;
                double.TryParse(str, out res);
            }
            catch { }
            return res;
        }
        static Dictionary<string, int> TrackNoCache = new Dictionary<string, int>();
        static object l = new object();
        public async static Task<int> GetTrackNo(string songId, string albumId)
        {
            if (TrackNoCache.ContainsKey(songId)) return TrackNoCache[songId];
            var json = await NetAccess.DownloadStringAsync(XiamiUrl.UrlAlbum(albumId));
            if (json == null) return 0;
            try
            {
                var songs = json.ToDynamicObject().album.songs;
                int i = 1;
                foreach (var song in songs)
                {
                    lock (l)
                    {
                        TrackNoCache[song.song_id.ToString()] = i;
                    }
                    i++;
                }
                return TrackNoCache[songId];
            }
            catch { return 0; }
        }
        public static dynamic ToDynamicObject(this string json)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            try
            {
                return serializer.Deserialize(json, typeof(object)) as dynamic;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using Metro.IoC;
using System.Windows;
using System.Collections;
using System.Net.Http;
using music_downloader.Data;
using Windows.Data.Json;
namespace music_downloader.Common
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
                        token.Cancel(true);
                    }
                }
            });
        }
        static List<CancellationTokenSource> cancelTokens = new List<CancellationTokenSource>();
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
                return JsonParser.Parse(json).GetObject()["lyric"].GetString();
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
                var str = JsonParser.Parse(json).GetObject()["year_play"].GetString();
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
                var songs = JsonParser.Parse(json).GetObject()["album"].GetObject()["songs"].GetArray();
                int i = 1;
                foreach (var song in songs)
                {
                    lock (l)
                    {
                        TrackNoCache[song.GetObject()["song_id"].GetString()] = i;
                    }
                    i++;
                }
                return TrackNoCache[songId];
            }
            catch { return 0; }
        }
    }
}

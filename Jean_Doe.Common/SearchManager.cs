using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using System.Threading;
using System.Windows.Controls;
using System.Collections;
namespace Jean_Doe.Common
{
    public class SearchManager
    {
        static EnumSearchState state = EnumSearchState.Finished;
        public static EnumSearchState State { get { return state; } }
        static void notifyState(SearchResult sr = null)
        {
            sr = sr ?? SearchResult.Empty;
            MessageBus.Instance.Publish(new MsgSearchStateChanged { State = state, SearchResult = sr });
        }
        public static async Task GetSongOfType(string id, EnumXiamiType type)
        {
            if (string.IsNullOrEmpty(id)) return;
            state = EnumSearchState.Started;
            notifyState();
            state = EnumSearchState.Working;
            var sr = await NetAccess.GetSongsOfType(id, type);
            /////////////
            notifyState(sr);
            state = EnumSearchState.Finished;
            notifyState();
        }

        public static async Task SearchAll(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            state = EnumSearchState.Started;
            notifyState();
            state = EnumSearchState.Working;
            string url = XiamiUrl.UrlSearchAll(key);
            string json = await NetAccess.DownloadStringAsync(url);
            /////////////
            dynamic obj = json.ToDynamicObject();
            if (obj != null)
            {
                var items = new List<IMusic>();
                foreach (var type in new string[] { "song", "album", "artist", "collect" })
                {
                    var data = obj[type + "s"] as ArrayList;
                    if (data == null) continue;
                    foreach (dynamic x in data)
                    {
                        items.Add(MusicFactory.CreateFromJson(x, (EnumXiamiType)Enum.Parse(typeof(EnumXiamiType), type)));
                    }
                }
                var sr = new SearchResult
                {
                    Items = items,
                    Keyword = key,
                    Page = -1,
                    SearchType = EnumSearchType.key
                };
                notifyState(sr);
            }
            state = EnumSearchState.Finished;
            notifyState();
        }
        public static async Task Search(string key, EnumXiamiType type = EnumXiamiType.song)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (type == EnumXiamiType.any)
            {
                await SearchAll(key);
                return;
            }
            state = EnumSearchState.Started;
            notifyState();
            int page = 1;
            while (true)
            {
                SearchResult sr = await NetAccess.Search(key, page, type);
                /////////////////////////////////////////
                if (sr == null || state == EnumSearchState.Cancelling)
                {
                    MessageBus.Instance.Publish("cancelling");
                    break;
                }
                if (state == EnumSearchState.Started)
                {
                    state = EnumSearchState.Working;
                    MessageBus.Instance.Publish(sr);
                }
                if (sr.Count == 0)
                {
                    MessageBus.Instance.Publish("no more results");
                    break;
                }
                notifyState(sr);
                page++;
            }
            state = EnumSearchState.Finished;
            notifyState();
        }
        public static async Task SearchByUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            state = EnumSearchState.Started;
            notifyState();
            state = EnumSearchState.Working;
            var sr = await NetAccess.SearchByUrl(url);
            if (sr != null && state != EnumSearchState.Cancelling)
            {
                MessageBus.Instance.Publish(sr);
                notifyState(sr);
            }
            state = EnumSearchState.Finished;
            notifyState();
        }
        //public static async Task SearchByType(string id, EnumXiamiType type)
        //{
        //    state = EnumSearchState.Started;
        //    notifyState();
        //    state = EnumSearchState.Working;
        //    var sr = await NetAccess.SearchByType(id, type);
        //    if(state == EnumSearchState.Cancelling)
        //    {
        //        return;
        //    }
        //    MessageBus.Instance.Publish(sr);
        //    notifyState(sr);
        //    state = EnumSearchState.Finished;
        //    notifyState();
        //}
        public static void Cancel()
        {
            state = EnumSearchState.Cancelling;
            NetAccess.CancelAsync();
        }
    }
    public enum EnumSearchState { Started, Working, Cancelling, Finished }
    public class MsgSearchStateChanged : IMessage
    {
        public EnumSearchState State;
        public SearchResult SearchResult;
    }
}

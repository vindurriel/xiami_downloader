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
        public static async Task GetSongOfType(string id, string type)
        {
			//if (string.IsNullOrEmpty(id)) return;
			//state = EnumSearchState.Started;
			//notifyState();
			//state = EnumSearchState.Working;
			//var sr = await NetAccess.GetSongsOfType(id, type);
			///////////////
			//notifyState(sr);
			//state = EnumSearchState.Finished;
			//notifyState();
        }

        public static async Task SearchAll(string key)
        {
			//if (string.IsNullOrEmpty(key)) return;
			//state = EnumSearchState.Started;
			//notifyState();
			//state = EnumSearchState.Working;
			//string url = XiamiUrl.UrlSearchAll(key);
			//string json = await NetAccess.DownloadStringAsync(url);
			///////////////
			//dynamic obj = json.ToDynamicObject();
			//if (obj != null)
			//{
			//	var items = new List<IMusic>();
			//	foreach (var type in new string[] { "song", "album", "artist", "collect" })
			//	{
			//		var data = obj[type + "s"] as ArrayList;
			//		if (data == null) continue;
			//		foreach (dynamic x in data)
			//		{
			//			items.Add(MusicFactory.CreateFromJson(x, type));
			//		}
			//	}
			//	var sr = new SearchResult
			//	{
			//		Items = items,
			//		Keyword = key,
			//		Page = -1,
			//		SearchType = EnumSearchType.key
			//	};
			//	notifyState(sr);
			//}
			//state = EnumSearchState.Finished;
			//notifyState();
        }
        public static async Task Search(string key, string type = "song")
        {
            if (string.IsNullOrEmpty(key)) return;
            if (type == "any")
            {
                await SearchAll(key);
                return;
            }
            state = EnumSearchState.Started;
            notifyState();
            int page = 1;
			ISearchProvider searchProvider = null;
            while (true)
            {
				SearchResult sr = await searchProvider.Search(key, page, type);
                /////////////////////////////////////////
                if (sr == null || state == EnumSearchState.Cancelling)
                {
                    break;
                }
                if (state == EnumSearchState.Started)
                {
                    state = EnumSearchState.Working;
                    MessageBus.Instance.Publish(sr);
                }
                if (sr.Count == 0)
                {
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
			ISearchProvider searchProvider = null;
			var sr = await searchProvider.SearchByUrl(url);
            if (sr != null && state != EnumSearchState.Cancelling)
            {
                MessageBus.Instance.Publish(sr);
                notifyState(sr);
            }
            state = EnumSearchState.Finished;
            notifyState();
        }
        public static void Cancel()
        {
            NetAccess.CancelAsync();
            state = EnumSearchState.Cancelling;
        }
    }
    public enum EnumSearchState { Started, Working, Cancelling, Finished }
    public class MsgSearchStateChanged : IMessage
    {
        public EnumSearchState State;
        public SearchResult SearchResult;
    }
}

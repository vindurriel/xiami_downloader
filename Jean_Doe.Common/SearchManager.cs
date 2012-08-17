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
using System.Text.RegularExpressions;
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
        public static async Task GetSongOfType(string id, EnumMusicType type)
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

        public static async Task SearchXiamiAll(string key)
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
                        items.Add(MusicFactory.CreateFromJson(x, (EnumMusicType)Enum.Parse(typeof(EnumMusicType), type)));
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
		public static async Task Search(string input)
		{
			if(string.IsNullOrEmpty(input)) return;
			var re_xiami=new Regex(@"xiami\.com");
			var re_douban=new Regex(@"douban\.com");
			if(re_xiami.IsMatch(input))
				await SearchByXiamiUrl(input);
			else if(re_douban.IsMatch(input))
				await SearchByDoubanUrl(input);
			else
				await SearchXiami(input);
		}  
		public static async Task SearchByDoubanUrl(string key) 
		{
			return;
		}
        public static async Task SearchXiami(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
			EnumMusicType type=EnumMusicType.song;
			Enum.TryParse(Global.AppSettings["SearchResultType"],out type);
            if (type == EnumMusicType.any)
            {
                await SearchXiamiAll(key);
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
        public static async Task SearchByXiamiUrl(string url)
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

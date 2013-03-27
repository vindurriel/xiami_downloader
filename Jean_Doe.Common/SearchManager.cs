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
        public static void notifyState(SearchResult sr = null)
        {
            sr = sr ?? SearchResult.Empty;
            MessageBus.Instance.Publish(new MsgSearchStateChanged { State = state, SearchResult = sr });
        }
     
        public static async Task GetMusic(string id, EnumSearchType type)
        {
            if (string.IsNullOrEmpty(id)) return;
			var key=string.Format("http://www.xiami.com/type/{0}/id/{1}",type.ToString(),id);
			await Search(key);
        }
        public static async Task Search(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            state = EnumSearchState.Started;
            notifyState();
            ISearchProvider provider=null;
            var re_source=new Regex(@"(from:\s*(\w+))");
            var m = re_source.Match(input);
            if (m.Success) {
                var type=m.Groups[2].Value.ToLower();
                switch (type)
                {
                    case "baidu": provider = new BaiduSearchProvider(); break;
                    case "xiami": provider = new XiamiSearchProvider(); break;
                    case "douban": provider = new DoubanSearchProvider(); break;
                    default:
                        break;
                }
                input = re_source.Replace(input, "");
            }
            if (provider == null)
                provider = new XiamiSearchProvider();
            state = EnumSearchState.Working;
            var sr=await provider.Search(input);
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
            state = EnumSearchState.Finished;
            notifyState();
        }
    }
    public enum EnumSearchState { Started, Working, Cancelling, Finished }
    public class MsgSearchStateChanged : IMessage
    {
        public EnumSearchState State;
        public SearchResult SearchResult;
    }
}

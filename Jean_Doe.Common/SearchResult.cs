using System.Collections.Generic;
using Artwork.MessageBus.Interfaces;
namespace Jean_Doe.Common
{
    public class SearchResult : IMessage
    {
        public string Keyword;
        public int Page;
        public int Count { get { if(Items == null)return 0; return Items.Count; } }
        public IList<IMusic> Items;
        public EnumSearchType SearchType;
        public bool HasNext;
        public static SearchResult Empty
        {
            get { return new SearchResult { Items = new List<IMusic>() }; }
        }
    }
}

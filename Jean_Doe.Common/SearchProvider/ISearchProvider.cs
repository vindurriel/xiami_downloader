using Jean_Doe.Common;
using System.Threading.Tasks;
interface ISearchProvider
{
    Task<SearchResult> Search(string key);
}
using Jean_Doe.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
interface ISearchProvider
{
    Task<SearchResult> Search(string key);
     Regex Pattern { get; }
}
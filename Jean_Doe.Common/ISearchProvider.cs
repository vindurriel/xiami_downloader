using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.Common
{
	interface ISearchProvider
	{
		 Task<SearchResult> Search(string key, int page, string type);
		 Task<SearchResult> SearchByUrl(string url);
	}
}

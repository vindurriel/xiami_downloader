using System.Threading.Tasks;
using Jean_Doe.Common;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Text.RegularExpressions;
public class XiamiSearchProvider : ISearchProvider
{
	public async Task<SearchResult> Search(string key)
	{
		if(string.IsNullOrEmpty(key))
			return null;
		if(Uri.IsWellFormedUriString(key, UriKind.Absolute))
			return await SearchByUrl(key);
		else
			return await SearchByKey(key);
	}
	static async Task<SearchResult> SearchByType(EnumMusicType type, string id)
	{
		string url = XiamiUrl.UrlPlaylistByIdAndType(id, type);
		if(type == EnumMusicType.artist)
		{
			url = XiamiUrl.UrlArtistTopSong(id);
		}
		var json = await NetAccess.DownloadStringAsync(url);
		///////////////////////////////////////////////////////
		if(json == null) return null;
		List<IMusic> items = new List<IMusic>();
		switch(type)
		{
			case EnumMusicType.album:
				items = GetSongsOfAlbum(json);
				break;
			case EnumMusicType.artist:
				items = GetSongsOfArtist(json);
				break;
			case EnumMusicType.collect:
				items = GetSongsOfCollect(json);
				break;
			case EnumMusicType.song:
				var song = await GetSong(id);
				if(song != null)
					items.Add(song);
				break;
			case EnumMusicType.any:
				break;
			default:
				break;
		}
		var res = new SearchResult
		{
			Items = items,
			Keyword = id,
			SearchType = EnumSearchType.type,
			Page = 1,
		};
		return res;
	}

	static async Task<SearchResult> SearchAll(string key)
	{
		string url = XiamiUrl.UrlSearchAll(key);
		string json = await NetAccess.DownloadStringAsync(url);
		/////////////
		dynamic obj = json.ToDynamicObject();
		var items = new List<IMusic>();
		foreach(var type in new string[] { "song", "album", "artist", "collect" })
		{
			var data = obj[type + "s"] as ArrayList;
			if(data == null) continue;
			foreach(dynamic x in data)
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
		return sr;
	}
	static async Task<SearchResult> SearchByKey(string key)
	{
		EnumMusicType type = EnumMusicType.song;
		Enum.TryParse(Global.AppSettings["SearchResultType"], out type);
		if(type == EnumMusicType.any)
		{
			return await SearchAll(key);
		}
		int page = 1;
		while(true)
		{
			SearchResult sr = await _search(key, page, type);
			/////////////////////////////////////////
			if(sr == null || sr.Count == 0 || SearchManager.State == EnumSearchState.Cancelling)
			{
				break;
			}

			SearchManager.notifyState(sr);
			page++;
		}
		return null;
	}
	async static Task<SearchResult> _search(string keyword, int page, EnumMusicType type = EnumMusicType.song)
	{
		string url = XiamiUrl.UrlSearch(keyword, page, type);
		string json = await NetAccess.DownloadStringAsync(url);
		/////////////
		if(json == null) return null;
		dynamic obj = json.ToDynamicObject();
		if(obj == null) return null;
		var data = obj.data as IList<dynamic>;
		if(data == null) return null;
		var items = new List<IMusic>();
		foreach(dynamic x in data)
		{
			items.Add(MusicFactory.CreateFromJson(x, type));
		}
		var res = new SearchResult
		{
			Items = items,
			Keyword = keyword,
			Page = page,
			SearchType = EnumSearchType.key
		};
		return res;
	}
	static async Task<SearchResult> SearchByUrl(string url)
	{
		var patterns = new List<Regex>{
                new Regex(@"(album|artist|song)/(\d+)"),//album,artist,song
                new Regex(@"show(collect)/id/(\d+)"),//collection
                new Regex(@"type/([^/]+?)/id/(\d+)"),//for fake urls to search by type
            };
		string strType = null, id = null;
		bool IsPatternRecognized = false;
		foreach(var pattern in patterns)
		{
			var j = pattern.Match(url);
			if(j.Success)
			{
				strType = j.Groups[1].Value;
				id = j.Groups[2].Value;
				IsPatternRecognized = true;
				break;
			}
		}
		if(!IsPatternRecognized)
			return null;
		EnumMusicType type = EnumMusicType.song;
		Enum.TryParse<EnumMusicType>(strType, out type);
		var res = await SearchByType(type, id);
		res.SearchType = EnumSearchType.url;
		res.Keyword = url;
		return res;
	}

	static List<IMusic> GetSongsOfArtist(string json)
	{
		var items = new List<IMusic>();
		try
		{
			var obj = json.ToDynamicObject().songs;
			foreach(var x in obj)
			{
				items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.song));
			}
		}
		catch { }
		{
		}
		return items;
	}
	static List<IMusic> GetSongsOfCollect(string json)
	{
		var items = new List<IMusic>();
		try
		{
			var obj = json.ToDynamicObject().collect.songs;
			foreach(var x in obj)
			{
				items.Add(MusicFactory.CreateFromJson(x, EnumMusicType.song));
			}
		}
		catch { }
		{
		}
		return items;
	}
	static List<IMusic> GetSongsOfAlbum(string json)
	{
		var items = new List<IMusic>();
		try
		{
			dynamic obj = json.ToDynamicObject();
			var album_name = obj.album.title;
			foreach(var x in obj.album.songs)
			{
				Song a = MusicFactory.CreateFromJson(x, EnumMusicType.song);
				a.AlbumName = album_name;
				items.Add(a);
			}
		}
		catch { }
		return items;
	}
	async static Task<Song> GetSong(string id)
	{
		var url = XiamiUrl.UrlPlaylistByIdAndType(id, EnumMusicType.song);
		Song song = null;
		try
		{
			var json = await NetAccess.DownloadStringAsync(url);
			////////
			if(json == null) return null;
			var obj = json.ToDynamicObject().song;
			song = MusicFactory.CreateFromJson(obj, EnumMusicType.song);
		}
		catch { }
		return song;
	}
	public async static Task<Album> GetAlbum(string id)
	{
		var url = XiamiUrl.UrlPlaylistByIdAndType(id, EnumMusicType.album);
		var json = await NetAccess.DownloadStringAsync(url);
		if(json == null) return null;
		var obj = json.ToDynamicObject().album;
		var Album = MusicFactory.CreateFromJson(obj, EnumMusicType.album);
		return Album;
	}
}
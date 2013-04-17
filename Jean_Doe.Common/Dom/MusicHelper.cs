using System.Collections.Generic;
using Jean_Doe.Common;
public  static class MusicHelper
{
	public static void LoadMusicInfoFromJson(IMusic m, dynamic obj)
	{
		var type = m.Type.ToString();
		m.Name = Get(obj,  type + "_name", "name","title" );
		m.Id = Get(obj, type + "_id", "id","obj_id" );
		m.Logo = StringHelper.EscapeUrl(Get(obj, type + "_logo", "logo","album_logo" ));
        m.JsonObject = obj as Dictionary<string, object>;
	}
    public static string Get(this IMusic m, params string[] props)
    {
        return Get(m.JsonObject, props);
    }
    public static string Get(dynamic obj, params string[] props)
    {
        object res = null;
        if (obj is DynamicJsonObject)
        {
			foreach(var prop in props)
			{
				res = obj[prop];
				if(res != null)
					break;
			}
        }
        else
        {
            var dict = obj as IDictionary<string, object>;
			foreach(var prop in props)
			{
				if(dict.ContainsKey(prop))
				{
					res = dict[prop];
					break;
				}
			}
        }
        return string.Format("{0}",res);
    }
}
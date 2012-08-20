using System.Collections.Generic;
using Jean_Doe.Common;
internal static class MusicHelper
{
    public static void LoadMusicInfoFromJson(IMusic m, dynamic obj)
    {
        var type = m.Type.ToString();
        m.Name = Get(obj, type+"_name","name");
		m.Id = Get(obj, type + "_id", "id");
        m.Logo =StringHelper.EscapeUrl(Get(obj,type + "_logo","logo"));
    }
    public static string Get(dynamic obj, string prop,string fallback=null)
    {
        object res = null;
        if (obj is DynamicJsonObject)
        {
            res = obj[prop];
			if(res == null && fallback != null)
				res = obj[fallback];
        }
        else
        {
            var dict = obj as IDictionary<string, object>;
            if (dict.ContainsKey(prop))
            {
				res = dict[prop];
            }
			else if(fallback!=null && dict.ContainsKey(fallback))
            {
				res = dict[fallback];
            }
        }

        return res as string;
    }
}
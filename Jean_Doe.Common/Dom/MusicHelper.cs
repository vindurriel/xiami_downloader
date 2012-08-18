using System.Collections.Generic;
using Jean_Doe.Common;
internal static class MusicHelper
{
    public static void LoadMusicInfoFromJson(IMusic m, dynamic obj)
    {
        var type = m.Type.ToString();
        m.Name = (Get(obj, type + "_name") as string);
        m.Id = Get(obj, type + "_id");
        m.Logo = (Get(obj, type + "_logo") as string).EscapeUrl();
    }
    public static string Get(dynamic obj, string prop)
    {
        string res = null;
        if (obj is DynamicJsonObject)
        {
            res = obj[prop];
        }
        else
        {
            var dict = obj as IDictionary<string, object>;
            if (dict.ContainsKey(prop))
            {
                res = dict[prop] as string;
            }
            else if (dict.ContainsKey(prop))
            {
                res = dict[prop] as string;
            }
        }

        return res;
    }
}
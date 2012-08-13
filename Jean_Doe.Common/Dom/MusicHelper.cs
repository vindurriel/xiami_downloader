using System.Collections.Generic;
using Jean_Doe.Common;
internal static class MusicHelper
{
    public static void LoadMusicInfoFromJson(IMusic m, dynamic obj)
    {
        var type = m.GetType().Name.ToLower().ToString();
        m.Name = (Get(obj, type, "name") as string);
        m.Id = Get(obj, type, "id");
        m.Logo = (Get(obj, type, "logo") as string).EscapeUrl();
    }
    public static string Get(dynamic obj, string type, string prop)
    {
        string res = null;
        if(obj is DynamicJsonObject)
        {
            res = obj[type + "_" + prop] ?? obj[prop];
        }
        else
        {
            var dict = obj as IDictionary<string, object>;
            if(dict.ContainsKey(type + "_" + prop))
            {
                res = dict[type + "_" + prop] as string;
            }
            else if(dict.ContainsKey(prop))
            {
                res = dict[prop] as string;
            }
        }

        return res;
    }
}
using System.Collections.Generic;
using Metro.IoC;
using music_downloader.Common;
using Windows.Data.Json;
namespace music_downloader.Data
{

    internal static class MusicHelper
    {
        public static void LoadMusicInfoFromJson(IMusic m, IJsonValue obj)
        {
            var type = m.Type.ToString();
            m.Name = Get(obj, type + "_name", "name");
            m.Id = Get(obj, type + "_id", "id");
            m.Logo = StringHelper.EscapeUrl(Get(obj, type + "_logo", "logo", "album_logo"));
        }
        public static string Get(IJsonValue obj, params string[] props)
        {
            object res = null;
            var o = obj.GetObject();
            foreach (var prop in props)
            {
                if (o.ContainsKey(prop))
                {
                    res = o[prop].GetString();
                    break;
                }
            }
            return string.Format("{0}", res);
        }
    }
}
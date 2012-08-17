using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.Common
{
    public class MusicFactory
    {
        public static IMusic CreateFromJson(dynamic obj, EnumMusicType type)
        {
            IMusic res = null;
            switch(type)
            {
                case EnumMusicType.album:
                    res = new Album();
                    break;
                case EnumMusicType.artist:
                    res = new Artist();
                    break;
                case EnumMusicType.collect:
                    res = new Collection();
                    break;
                case EnumMusicType.song:
                    res = new Song();
                    break;
                default:
                    break;
            }
            if(res != null)
                res.CreateFromJson(obj);
            return res;
        }
    }
}

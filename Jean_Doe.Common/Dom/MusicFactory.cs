using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.Common
{
    public class MusicFactory
    {
        public static IMusic CreateFromJson(dynamic obj, EnumXiamiType type)
        {
            IMusic res = null;
            switch(type)
            {
                case EnumXiamiType.album:
                    res = new Album();
                    break;
                case EnumXiamiType.artist:
                    res = new Artist();
                    break;
                case EnumXiamiType.collect:
                    res = new Collection();
                    break;
                case EnumXiamiType.song:
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

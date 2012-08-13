using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.Common
{
    public class MusicFactory
    {
        public static IMusic CreateFromJson(dynamic obj, string type)
        {
            IMusic res = null;
            switch(type)
            {
                case "album":
                    res = new Album();
                    break;
                case "artist":
                    res = new Artist();
                    break;
                case "collect":
                    res = new Collection();
                    break;
                case "song":
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

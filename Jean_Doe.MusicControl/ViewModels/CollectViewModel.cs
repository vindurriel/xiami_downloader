using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    public class CollectViewModel : MusicViewModel
    {
        Collection collect;
        public CollectViewModel(Collection a)
            : base(a)
        {
            collect = a;
            TypeImage = "\xE142";
            InitLogo(string.Format("collect_{0}.jpg", Id));
        }
        public string ArtistName { get { return collect.Get("user_name"); } }
        public string AlbumName { get { return "的精选集"; } }
        public new string Description { get { return collect.Get("description"); } }
    }
}

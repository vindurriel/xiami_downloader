using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.MusicControl
{
    public class ArtistViewModel : MusicViewModel, IHasArtist
    {
        Artist artist;
        public ArtistViewModel(Artist a):base(a)
        {
            artist = a;
        }
        public string TypeColor { get { return "#78a674"; } }
        public string ArtistName { get { return artist.Name; } }

        public string ArtistId
        {
            get { return artist.Id; }
        }
    }
}

using Jean_Doe.Common;
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
            TypeImage = "\xe13d";
            ImageManager.Get(string.Format("artist_{0}.jpg", a.Id), a.Logo, ApplyLogo);
        }
        public string ArtistName { get { return "艺术家"; } }
        public string AlbumName { get { return artist.AlbumCount; } }
        public string ArtistId
        {
            get { return artist.Id; }
        }
    }
}

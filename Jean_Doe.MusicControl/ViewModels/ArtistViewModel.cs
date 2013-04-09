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
            ImageSource = "/Jean_Doe.MusicControl;component/Resources/artist.png";
            typecolor = "#78a674";
        }
        public string ArtistName { get { return artist.AlbumCount; } }
        public string AlbumName { get { return artist.Description; } }
        public string ArtistId
        {
            get { return artist.Id; }
        }
    }
}

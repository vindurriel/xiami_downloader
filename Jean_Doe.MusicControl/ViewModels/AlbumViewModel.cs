using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    public class AlbumViewModel : MusicViewModel, IHasAlbum,IHasArtist
    {
        Album album;
        public AlbumViewModel(Album a):base(a)
        {
            album = a;
            TypeImage = ImageSource = "/Jean_Doe.MusicControl;component/Resources/album.png";
        }
        public string AlbumName { get { return album.Name; } }
        public string ArtistName { get { return album.ArtistName; } }
        public string AlbumId
        {
            get { return album.Id; }
        }
        public string ArtistId
        {
            get { return album.ArtistId; }
        }
    }
}

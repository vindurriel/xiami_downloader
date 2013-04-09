using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    public class AlbumViewModel : MusicViewModel, IHasAlbum
    {
        Album album;
        public AlbumViewModel(Album a):base(a)
        {
            album = a;
            ImageSource = "/Jean_Doe.MusicControl;component/Resources/album.png";
            typecolor = "#c3b775";
        }
        public string AlbumName { get { return album.Description; } }
        public string ArtistName { get { return album.ArtistName; } }

        public string AlbumId
        {
            get { return album.Id; }
        }
    }
}

using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Jean_Doe.MusicControl
{
    public class AlbumViewModel : MusicViewModel, IHasAlbum, IHasArtist
    {
        Album album;
        public AlbumViewModel(Album a)
            : base(a)
        {
            album = a;
            TypeImage = "\xE1d2";
            ImageManager.Get(string.Format("album_{0}.jpg", album.Id), a.Logo, ApplyLogo);
        }
        public string AlbumName { get { return "的专辑"; } }
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

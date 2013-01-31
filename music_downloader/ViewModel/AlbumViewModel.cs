using music_downloader.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace music_downloader
{
    public class AlbumViewModel : MusicViewModel, IHasAlbum
    {
        Album album;
        public AlbumViewModel(Album a):base(a)
        {
            album = a;
        }
        public string AlbumName { get { return album.Name; } }

        public string AlbumId
        {
            get { return album.Id; }
        }
    }
}

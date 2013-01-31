using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace music_downloader
{
    interface IHasArtist
    {
        string ArtistId { get; }
        string ArtistName { get; }
    }
    interface IHasAlbum
    {
        string AlbumId { get; }
        string AlbumName { get; }
    }
    interface IHasCollection
    {
        string CollectionId { get; }
        string CollectionName { get; }
    }
}

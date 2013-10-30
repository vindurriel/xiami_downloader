using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.MusicControl
{
    public interface IHasMusicPart { }
    public interface IHasArtist:IHasMusicPart
    {
        string ArtistId { get; }
        string ArtistName { get; }
    }
    public interface IHasAlbum : IHasMusicPart
    {
        string AlbumId { get; }
        string AlbumName { get; }
    }
}

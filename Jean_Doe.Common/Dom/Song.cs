using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Jean_Doe.Common
{
    public class Song : IMusic
    {
        #region xml properties
        public string UrlMp3 { get; set; }
        public string UrlArt { get; set; }
        public string UrlLrc { get; set; }
        public string FilePath { get; set; }
        public int TrackNo { get; set; }

        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        private string albumId;

        public string AlbumId
        {
            get { return albumId; }
            set { albumId = value; }
        }
        private string artistId;

        public string ArtistId
        {
            get { return artistId; }
            set { artistId = value; }
        }
        private bool inFav = false;
        [XmlAttribute]
        public bool InFav
        {
            get { return inFav; }
            set { inFav = value; }
        }
        private bool hasLrc;
        [XmlAttribute]
        public bool HasLrc
        {
            get { return hasLrc; }
            set { hasLrc = value; }
        }
        private bool hasMp3;
        [XmlAttribute]
        public bool HasMp3
        {
            get { return hasMp3; }
            set { hasMp3 = value; }
        }

        private bool hasArt;
        [XmlAttribute]
        public bool HasArt
        {
            get { return hasArt; }
            set { hasArt = value; }
        }
        [XmlElement("Title")]
        public XmlCDataSection XTitle
        {
            get { return new XmlDocument().CreateCDataSection(Name); }
            set { Name = value.Value; }
        }
        [XmlElement("Artist")]
        public XmlCDataSection XArtist
        {
            get { return new XmlDocument().CreateCDataSection(ArtistName); }
            set { ArtistName = value.Value; }
        }
        [XmlElement("Album")]
        public XmlCDataSection XAlbum
        {
            get { return new XmlDocument().CreateCDataSection(AlbumName); }
            set { AlbumName = value.Value; }
        }
        #endregion
        #region Properties
        [XmlIgnore]
        public dynamic  JsonObject { get; set; }
        [XmlIgnore]
        public string Description { get; set; }
        [XmlIgnore]
        public string Logo { get { return UrlArt; } set { UrlArt = value; } }
        [XmlIgnore]
        public EnumMusicType Type { get { return EnumMusicType.song; } }
        [XmlIgnore]
        public bool WriteId3 { get; set; }
        [XmlIgnore]
        public string Name { get; set; }
        private string artistName;
        [XmlIgnore]
        public string ArtistName
        {
            get { return artistName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    artistName = value; return;
                }
                var list = value.Split(';').ToList();
                if (list.Count > 1)
                {
                    artistName = list[0];
                    list.RemoveAt(0);
                    FeaturingArtists = string.Join(";", list);
                }
                else
                    artistName = value;
            }
        }
        [XmlIgnore]
        public string FeaturingArtists { get; set; }
        [XmlIgnore]
        public string AlbumName { get; set; }
        #endregion
        public void CreateFromJson(dynamic obj)
        {
            MusicHelper.LoadMusicInfoFromJson(this, obj);
            ArtistId = MusicHelper.Get(obj, "artist_id");
            ArtistName = MusicHelper.Get(obj, "artist_name", "name");
            AlbumId = MusicHelper.Get(obj, "album_id");
            AlbumName = MusicHelper.Get(obj, "album_name", "name");
            UrlMp3 = StringHelper.EscapeUrl(MusicHelper.Get(obj, "location", "song_location", "listen_file"));
            UrlLrc = StringHelper.EscapeUrl(MusicHelper.Get(obj, "lyric", "song_lyric"));
            Description = MusicHelper.Get(obj, "reason", "description");
            WriteId3 = true;
        }
    }
    [XmlRoot("Songs")]
    public class Songs : List<Song>
    {
    }
}
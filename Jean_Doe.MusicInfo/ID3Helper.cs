using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HundredMilesSoftware.UltraID3Lib;
using System.Drawing;

namespace Jean_Doe.MusicInfo
{
    public enum EnumTags { Artist, Album, Title, Id, TrackNo }
    public class ID3Helper
    {
        private UltraID3 id3info;
        private ID3v2Tag id3v2tag;

        public ID3Helper(string filename)
        {
            id3info = new UltraID3();
            id3info.Read(filename);
            id3info.ID3v1Tag.Clear();
            id3v2tag = id3info.ID3v2Tag;
        }
        public ID3Helper(ID3v2Tag tags)
        {
            id3v2tag = tags;
        }
        public string GetTag(EnumTags tag)
        {
            var res = "";
            switch (tag)
            {
                case EnumTags.Artist:
                    res = id3info.Artist;
                    break;
                case EnumTags.Album:
                    res = id3info.Album;
                    break;
                case EnumTags.Title:
                    res = id3info.Title;
                    break;
                case EnumTags.Id:
                    res = id3info.Comments;
                    break;
                default:
                    break;
            }
            return res;
        }


        public void SetTitle(string title)
        {
            ID3v23TitleFrame titleFrame = new ID3v23TitleFrame(TextEncodingTypes.Unicode);
            titleFrame.Title = title;
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.Title);
            id3v2tag.Frames.Add(titleFrame);
        }

        public void SetAlbum(string album)
        {
            ID3v23AlbumFrame albumFrame = new ID3v23AlbumFrame(TextEncodingTypes.Unicode);
            albumFrame.Album = album;
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.Album);
            id3v2tag.Frames.Add(albumFrame);
        }

        public void SetArtist(string artist)
        {
            ID3v23ArtistFrame artistFrame = new ID3v23ArtistFrame(TextEncodingTypes.Unicode);
            artistFrame.Artist = artist;
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.Artist);
            id3v2tag.Frames.Add(artistFrame);
        }
        public void SetComments(string comment)
        {
            var frame = new ID3v23CommentsFrame(TextEncodingTypes.Unicode);
            frame.Comments = comment;
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.Comments);
            id3v2tag.Frames.Add(frame);
        }

        public void SetYear(short year)
        {
            if (year > 0)
            {
                ID3v23YearFrame yearFrame = new ID3v23YearFrame();
                yearFrame.Year = year;
                id3v2tag.Frames.Add(yearFrame);
            }
        }
        public void SetTrackNo(short num)
        {

            var frame = new ID3v23TrackNumFrame { TrackNum = num };
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.TrackNum);
            id3v2tag.Frames.Add(frame);
        }
        public void SetCover(Bitmap picture)
        {
            ID3v23PictureFrame pictureFrame = new ID3v23PictureFrame
                                                  {
                                                      MIMEType = "image/jpeg",
                                                      Picture = picture,
                                                      PictureType = PictureTypes.CoverFront
                                                  };
            id3v2tag.Frames.Remove(CommonID3v2FrameTypes.Picture);
            id3v2tag.Frames.Add(pictureFrame);
        }

        public void SetAlbumArtist(string albumArtist)
        {
            ID3v23BandFrame albumArtistFrame = new ID3v23BandFrame(TextEncodingTypes.Unicode) {Band = albumArtist};
            id3v2tag.Frames.Add(albumArtistFrame);
        }


        public void SetLyrics(Lyrics lyrics)
        {
            if (lyrics == null)
            {
                return;
            }
            ID3v23SynchronizedLyricsFrame lyricsFrame = new ID3v23SynchronizedLyricsFrame(TextEncodingTypes.Unicode);
            List<LyricsUnit> lyList = lyrics.GetLyris();
            foreach (LyricsUnit t in lyList)
            {
                lyricsFrame.SynchronizedLyrics.Add(t.Lyrics, t.TimeStamp);
            }
            id3v2tag.Frames.Add(lyricsFrame);
        }

        public void SetUnSyncLyrics(Lyrics lyrics)
        {
            if (lyrics == null)
            {
                return;
            }
            ID3v23UnsynchedLyricsFrame lyricsFrame = new ID3v23UnsynchedLyricsFrame(TextEncodingTypes.Unicode);
            string str = "";
            List<LyricsUnit> lyList = lyrics.GetSortedLyrics();
            for (int i = 0; i < lyList.Count; i++)
            {
                str = str + lyList[i].Lyrics + "\n";
            }
            lyricsFrame.UnsynchedLyrics = str;
            id3v2tag.Frames.Add(lyricsFrame);
        }

        public void Commit()
        {
            try
            {
                id3info.Write();
            }
            catch (Exception)
            {
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HundredMilesSoftware.UltraID3Lib;
using System.Drawing;
namespace Jean_Doe.MusicInfo
{
    public class MusicInfo
    {
        UltraID3 Tags;
        ID3Helper id3Helper;
        public MusicInfo(string file)
        {
            Tags = new UltraID3();
            Tags.Read(file);
            Tags.ID3v1Tag.Clear();
            id3Helper = new ID3Helper(Tags.ID3v2Tag);
        }
        public void Commit()
        {
            try
            {
                Tags.Write();
            }
            catch (Exception)
            {
            }
        }
        public string DumpInfo()
        {
            return string.Format("{0} {1} {2} {3} {4}", Title, Artist, Id, Album, TrackNo);
        }

        public bool HasCover()
        {
            var cover = Tags.ID3v2Tag.Frames.GetFrames(CommonMultipleInstanceID3v2FrameTypes.Picture);
            return cover != null;
        }
        public bool HasId()
        {
            long o;
            return long.TryParse(Id, out o);
        }
        public string Artist { get { return Tags.Artist; } set { if (!string.IsNullOrEmpty(value)) id3Helper.SetArtist(value); } }
        public string Album { get { return Tags.Album; } set { if (!string.IsNullOrEmpty(value)) id3Helper.SetAlbum(value); } }
        public string Title { get { return Tags.Title; } set { if (!string.IsNullOrEmpty(value)) id3Helper.SetTitle(value); } }
        public string Id { get { return Tags.Comments; } set { if (!string.IsNullOrEmpty(value)) id3Helper.SetComments(value); } }
        public string TrackNo
        {
            get { return Tags.TrackNum == null ? null : Tags.TrackNum.ToString(); }
            set
            {
                short i;
                if (short.TryParse(value, out i))
                    id3Helper.SetTrackNo(i);
            }
        }
        public string Cover
        {
            get
            {
                return HasCover().ToString();
            }
            set
            {
                try
                {
                    var picture = (Bitmap)Bitmap.FromFile(value);
                    id3Helper.SetCover(picture);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

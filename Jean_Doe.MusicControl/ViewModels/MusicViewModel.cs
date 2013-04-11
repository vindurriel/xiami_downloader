using System.ComponentModel;
using Jean_Doe.Common;
using System.Collections.Generic;
namespace Jean_Doe.MusicControl
{
    public class MusicViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion
        IMusic music;
        private string imageSrc;
        public string ImageSource
        {
            get { return imageSrc; }
            set
            {
                imageSrc = value; Notify("ImageSource");
                LogoColor = ImageHelper.GetAverageColor(value);
            }
        }
        protected string typeImage;
        public string TypeImage { get { return typeImage; } set { typeImage = value; Notify("TypeImage"); } }
        protected string logoColor = ImageHelper.DefaultColor;
        public string LogoColor { get { return logoColor; } set { logoColor = value; Notify("LogoColor"); } }
        public MusicViewModel(IMusic m)
        {
            music = m;
        }
        private string searchStr = null;

        public string SearchStr
        {
            get
            {
                if (searchStr == null)
                {
                    var text = new List<string>();
                    text.Add(Name.ToLower());
                    if (this is IHasAlbum)
                        text.Add(((IHasAlbum)this).AlbumName.ToLower());
                    if (this is IHasArtist)
                        text.Add(((IHasArtist)this).ArtistName.ToLower());
                    searchStr = string.Join(" ", text);
                }
                return searchStr;
            }
        }

        public virtual string Name { get { return music.Name; } }
        public virtual string Id { get { return music.Id; } }
        public virtual string Logo { get { return music.Logo; } }
        public virtual EnumMusicType Type { get { return music.Type; } }
    }
}

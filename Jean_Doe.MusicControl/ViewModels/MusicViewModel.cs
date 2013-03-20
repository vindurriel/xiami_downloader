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
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion
        IMusic music;
        private string imageSrc="/Jean_Doe.MusicControl;component/Resources/nocover.png";
        public string ImageSource
        {
            get { return imageSrc; }
            set { imageSrc = value; Notify("ImageSource"); }
        }
        public MusicViewModel(IMusic m)
        {
            music = m;
        }
        private string searchStr=null;

        public string SearchStr
        {
            get
            {
                if (searchStr == null) {
                    var text = new List<string>();
                    text.Add(Name.ToLower());
                    if (this is IHasAlbum)
                        text.Add(((IHasAlbum)this).AlbumName.ToLower());
                    if (this is IHasArtist)
                        text.Add(((IHasArtist)this).ArtistName.ToLower());
                    searchStr=string.Join(" ", text);
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

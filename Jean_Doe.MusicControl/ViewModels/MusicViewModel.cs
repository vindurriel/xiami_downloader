using System.ComponentModel;
using Jean_Doe.Common;
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
        public virtual string Name { get { return music.Name; } }
        public virtual string Id { get { return music.Id; } }
        public virtual string Logo { get { return music.Logo; } }
        public virtual EnumXiamiType Type { get { return music.Type; } }
    }
}

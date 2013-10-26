using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// UserActionsControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserActionsControl : INotifyPropertyChanged
    {
        public UserActionsControl()
        {
            InitializeComponent();
            Global.ListenToEvent("xiami_avatar", SetAvatar);
        }

        BitmapSource avatar;
        public BitmapSource Avatar { get { return avatar; } set { avatar = value; Notify("Avatar"); } }
        void SetAvatar(string s)
        {
            ImageManager.Get(string.Format("user_{0}.jpg", Global.AppSettings["xiami_uid"]), s, (img) => Avatar = img);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}

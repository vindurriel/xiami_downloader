using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Jean_Doe.MusicControl
{
    internal class MyStyleSelector:DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var o = ((container as ContentPresenter).Parent as DataGridCell).DataContext as MusicViewModel;
            if(o == null) return null;
            DataTemplate  res = null;
            switch(o.Type)
            {
                case Jean_Doe.Common.EnumXiamiType.album:
                    res = AlbumTemplate;
                    break;
                case Jean_Doe.Common.EnumXiamiType.artist:
                    res = ArtistTemplate;
                    break;
                case Jean_Doe.Common.EnumXiamiType.collect:
                    res = CollectionTemplate;
                    break;
                case Jean_Doe.Common.EnumXiamiType.song:
                    res = SongTemplate;
                    break;
                default:
                    break;
            }
            return res;
        }
        
        public DataTemplate SongTemplate { get; set; }
        public DataTemplate ArtistTemplate { get; set; }
        public DataTemplate AlbumTemplate { get; set; }
        public DataTemplate CollectionTemplate { get; set; }
    }
}

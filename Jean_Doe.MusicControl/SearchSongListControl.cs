using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;

namespace Jean_Doe.MusicControl
{
    public class SearchSongListControl : SongListControl,
        IHandle<MsgSearchStateChanged>
    {
        public SearchSongListControl()
        {
            MessageBus.Instance.Subscribe(this);
        }
        protected override async void link_album(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as SongViewModel;
            if(t == null) return;
            var id = t.AlbumId;
            await SearchManager.SearchByType(id, EnumXiamiType.album);
        }
        protected override async void link_artist(object sender, RoutedEventArgs e)
        {
            var t = (sender as Hyperlink).DataContext as SongViewModel;
            if(t == null) return;
            await SearchManager.Search(t.Artist,EnumXiamiType.artist);
        }
        public void AddItems(IEnumerable<IMusic> items)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Items.AddItems(items);
            }));
        }

        public void Handle(MsgSearchStateChanged message)
        {
            switch(message.State)
            {
                case EnumSearchState.Started:
                    SetBusy(true);
                    Clear();
                    break;
                case EnumSearchState.Working:
                    AddItems(message.SearchResult.Items);
                    break;
                case EnumSearchState.Finished:
                    SetBusy(false);
                    break;
                default:
                    break;
            }
        }
    }
   
   
}

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
        public void AddItems(IEnumerable<IMusic> items)
        {
            Items.AddItems(items);
        }

        public void Handle(MsgSearchStateChanged message)
        {
            switch(message.State)
            {
                case EnumSearchState.Started:
                    Clear();
                    break;
                case EnumSearchState.Working:
                    AddItems(message.SearchResult.Items);
                    break;
                case EnumSearchState.Finished:
                    break;
                default:
                    break;
            }
        }
    }


}

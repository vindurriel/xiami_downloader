using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artwork.MessageBus.Interfaces;
namespace Jean_Doe.Downloader
{
    public class MsgDownloadStateChanged : IMessage
    {
        public EnumDownloadState State;
        public string Message;
        public string Id;
        public object Item;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artwork.MessageBus.Interfaces;
namespace Jean_Doe.Downloader
{
    public class MsgDownloadProgressChanged:IMessage
    {
        public int Percent;
        public string Id;
    }
}

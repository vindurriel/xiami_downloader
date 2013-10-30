using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;
using Jean_Doe.MusicControl;
using Jean_Doe.Downloader;
using System.Text.RegularExpressions;
namespace Jean_Doe.MusicControl
{
    public sealed class DownloaderArt : Downloader.Downloader
    {
        public override void Process()
        {
            base.Process();
			//update cover in song list
			AfterDownloadManager.Fire(Info.Id, Info.FileName);
        }
    }
}

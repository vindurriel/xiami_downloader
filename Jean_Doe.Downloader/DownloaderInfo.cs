using System;

namespace Jean_Doe.Downloader
{
    public enum EnumDownloadType { Text, Bytes };
	public class DownloaderInfo
    {
        public string Id;
        public string Url;
        public string FileName;
        public string Tag;
        public object Entity;
        public long TotalBytes;
		public int Priority;
    }
}

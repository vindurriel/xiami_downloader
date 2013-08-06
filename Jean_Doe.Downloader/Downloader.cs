using System;
using System.ComponentModel;
using System.Net;
using Artwork.MessageBus;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
namespace Jean_Doe.Downloader
{
	[DebuggerDisplay("Id={Id}")]
    public class Downloader
    {
        public virtual bool CanDownload { get { return true; } }
        public virtual bool CanProcess { get { return true; } }
        protected long curBytes = -1;
        protected long totalBytes = -1;
        long start = -1;
        private EnumDownloadState state;

        public EnumDownloadState State
        {
            get { return state; }
            set
            {

                if (state == value) return;
                state = value;
                NotifyState();
            }
        }
		public void NotifyError(Exception e)
		{
			state = EnumDownloadState.Error;
			e.Source += string.Format(" downloader id:{0}", Info.Id);
			Logger.Error(e);
			NotifyState(e.Message);
		}
        public void NotifyState(string s = null)
        {
            MessageBus.Instance.Publish(new MsgDownloadStateChanged { Id = Info.Tag, State = State, Message = s,Item=Info.Entity });
        }
        public string Id { get { return Info.Id; } }
        public DownloaderInfo Info { get; set; }
        bool canceled = false;
        static readonly int bufferSize = 1024 * 64;
        async Task<long> getContentLength(string url)
        {
            WebResponse response = null;
            long size = -1;
            try
            {
                response = await WebRequest.CreateHttp(url).GetResponseAsync();
                size = response.ContentLength;
            }
			catch(Exception e) { throw e; }
            finally
            {
                if (response != null)
                    response.Close();
            }
            return size;

        }
        public async virtual Task Download()
        {
            try
            {
				if(Info.Url == null )
				{
					throw new Exception(string.Format("\'{0}\' is not a valid url", Info.Url));
				}
				var dir = Path.GetDirectoryName(Info.FileName);
				if(!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				long contentLength = DownloadManager.Instance.GetContentLength(Info.Url);
				if(contentLength == -1)
				{
					contentLength = await this.getContentLength(Info.Url);
					///////////////////////////
					DownloadManager.Instance.SetContentLength(Info.Url, contentLength);
				}
				start = 0;
				if(File.Exists(Info.FileName))
				{
					var x = new FileInfo(Info.FileName);
					start = x.Length;
				}
				//already downloaded
                if (start == contentLength)
                {
                    curBytes = totalBytes = start;
                    OnDownloaded();
                    return;
                }
                else 
                {
                    start = 0;
                }
				var request = WebRequest.CreateHttp(Info.Url);
				request.Credentials = CredentialCache.DefaultCredentials;
				if(start > 0)
					request.AddRange(start);
				HttpWebResponse response = null;
                response = await request.GetResponseAsync() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.PartialContent || response.ContentLength == -1)
                {
                    File.Delete(Info.FileName);
                    start = 0;
                }
                curBytes = start;
                totalBytes = response.ContentLength + start;
                var buffer = new Byte[bufferSize];
                using (var stream = response.GetResponseStream())
                {
                    var readCount = -1;
                    do
                    {
                        if (canceled)
                        {
                            break;
                        }
                        readCount = await stream.ReadAsync(buffer, 0, bufferSize);
                        if (canceled)
                        {
                            break;
                        }
                        curBytes += readCount;
                        using (var file = File.Open(Info.FileName, FileMode.Append,FileAccess.Write,FileShare.None))
                        {
                            file.Write(buffer, 0, readCount);
                        }
                        OnProgressChanged();
                    }
                    while (readCount > 0);
                    this.OnDownloaded();
                }
            }
            catch (Exception e)
            {
                NotifyError(e);
            }
        }
        public virtual void Process()
        {
        }
        protected virtual void OnProgressChanged()
        {
        }
		protected virtual void OnDownloaded()
		{

			while(!CanProcess)
			{
				if(canceled) break;
				Thread.Sleep(500);
			}
			if(canceled)
			{
				State = EnumDownloadState.Cancel;
				return;
			}
			State = EnumDownloadState.Processing;
			Process();
			State = EnumDownloadState.Success;
		}

        public void StopDownload()
        {
            if (state >= EnumDownloadState.Processing)
                return;
            canceled = true;
            State = EnumDownloadState.Cancel;
        }
        public void StartDownload()
        {
            State = EnumDownloadState.Downloading;
            canceled = false;
            new Thread(new ThreadStart(async () => { await Download(); })).Start();
        }
       
        public int Percent
        {
            get
            {
                if (curBytes <= 0 || totalBytes <= 0) return 0;
                return (int)(100 * curBytes / (double)totalBytes);
            }
        }
    }
}

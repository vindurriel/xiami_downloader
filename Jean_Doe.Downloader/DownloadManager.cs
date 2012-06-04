using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using Artwork.MessageBus;
using System.Collections;
//using Jean_Doe.Common;
namespace Jean_Doe.Downloader
{
    public class DownloadManager
    {
        #region properties
        int maxThread = 100;
        public int Count { get { return pool.Count; } }
        public int DownloadingCount { get { return pool.Values.Count(x => x.State == EnumDownloadState.Downloading); } }
        public int WaitingCount { get { return pool.Values.Count(x => x.State == EnumDownloadState.Waiting); } }
        readonly Dictionary<string, Downloader> pool = new Dictionary<string, Downloader>();
        readonly Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
        readonly Dictionary<string, long> contentLengthOfUrl = new Dictionary<string, long>();
        DispatcherTimer dt = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        #endregion
        private DownloadManager()
        {
            SetMaxConnection(maxThread);
            dt.Tick += (s, e) => spinWait();
        }
        static DownloadManager inst;
        public static DownloadManager Instance { get { if(inst == null) inst = new DownloadManager(); return inst; } }
        public void SetMaxConnection(int maxConn)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = maxConn;
            maxThread = maxConn;
        }
        object lockContentLength = new object();
        public long GetContentLength(string url)
        {
            if(string.IsNullOrEmpty(url)) return -1;
            long res = -1;
            if(contentLengthOfUrl.ContainsKey(url))
                res = contentLengthOfUrl[url];
            return res;
        }
        public void SetContentLength(string url, long length)
        {
            if(string.IsNullOrEmpty(url)) return;
            lock(lockContentLength)
            {
                if(contentLengthOfUrl.ContainsKey(url))
                    contentLengthOfUrl[url] = length;
                else
                    contentLengthOfUrl.Add(url, length);
            }
        }
        #region downloader control
        void stop(string id)
        {
            if(!pool.ContainsKey(id)) return;
            pool[id].StopDownload();
        }
        public void Stop(List<string> taglist)
        {
            foreach(var tag in taglist)
            {
                if(!tags.ContainsKey(tag))
                    continue;
                foreach(var id in tags[tag])
                {
                    stop(id);
                }
            }
        }
        void start(string id)
        {
            if(!pool.ContainsKey(id)) return;
            var d = pool[id];
            if(d.State == EnumDownloadState.Downloading || d.State == EnumDownloadState.Processing) return;
            pool[id].BeginWait();
        }
        public Downloader GetDownloader(string id)
        { return pool.Values.FirstOrDefault(x => x.Id == id); }
        public void Start(List<string> taglist)
        {
            startSpin();
            foreach(var tag in taglist)
            {
                if(!tags.ContainsKey(tag))
                    continue;
                foreach(var id in tags[tag])
                {
                    start(id);
                }
            }
        }

        void remove(string id)
        {
            if(!pool.ContainsKey(id)) return;
            pool[id].StopDownload();
            pool.Remove(id);
        }
        public void Remove(List<string> taglist)
        {
            foreach(var tag in taglist)
            {
                if(!tags.ContainsKey(tag)) continue;
                foreach(var id in tags[tag])
                {
                    remove(id);
                }
                tags.Remove(tag);
            }
        }
        public void Add(Downloader downloader)
        {
            if(downloader.Info != null && pool.ContainsKey(downloader.Info.Id))
                return;
            pool.Add(downloader.Info.Id, downloader);
            if(downloader.Info.Tag != null)
            {
                if(!tags.ContainsKey(downloader.Info.Tag))
                    tags.Add(downloader.Info.Tag, new List<string>());
                tags[downloader.Info.Tag].Add(downloader.Info.Id);
            }
        }
        #endregion
        #region concurrency management
        void startSpin()
        {
            if(!dt.IsEnabled)
            {
                dt.Start();
                MessageBus.Instance.Publish(new MsgSetBusy { On = true });
            }
        }
        void endSpin()
        {
            if(dt.IsEnabled)
            {
                dt.Stop();
                MessageBus.Instance.Publish(new MsgSetBusy { On = false });

            }
        }
        void spinWait()
        {
            var a = Count;
            while(WaitingCount > 0 && DownloadingCount < maxThread)
            {
                var first = pool.Values.FirstOrDefault(x =>
                    x.State == EnumDownloadState.Waiting
                    && x.CanDownload
                    );
                if(first == null) return;
                var downloader = first;
                downloader.StartDownload();
            }
            if(WaitingCount == 0 && DownloadingCount == 0)
            {
                endSpin();
            }
        }
        #endregion
    }
}
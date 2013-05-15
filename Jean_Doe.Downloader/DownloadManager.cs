using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using Artwork.MessageBus;
using System.Collections;
using Jean_Doe.Common;
//using Jean_Doe.Common;
namespace Jean_Doe.Downloader
{
    public class DownloadManager
    {
        #region properties
        int maxThread = 100;
        readonly Dictionary<string, Downloader> pool = new Dictionary<string, Downloader>();
        readonly Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
        readonly Dictionary<string, long> contentLengthOfUrl = new Dictionary<string, long>();
        DispatcherTimer dt = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        #endregion
        private DownloadManager()
        {
            SetMaxConnection(maxThread);
            dt.Tick += (s, e) => pick();
        }
        static DownloadManager inst;
        public static DownloadManager Instance { get { if (inst == null) inst = new DownloadManager(); return inst; } }
        public void SetMaxConnection(int maxConn)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = maxConn;
            maxThread = maxConn;
        }
        object lockContentLength = new object();
        public long GetContentLength(string url)
        {
            if (string.IsNullOrEmpty(url)) return -1;
            long res = -1;
            if (contentLengthOfUrl.ContainsKey(url))
                res = contentLengthOfUrl[url];
            return res;
        }
        public void SetContentLength(string url, long length)
        {
            if (string.IsNullOrEmpty(url)) return;
            lock (lockContentLength)
            {
                if (contentLengthOfUrl.ContainsKey(url))
                    contentLengthOfUrl[url] = length;
                else
                    contentLengthOfUrl.Add(url, length);
            }
        }
        #region private control methods
        void stop(string id)
        {
            if (!pool.ContainsKey(id)) return;
            pool[id].StopDownload();
        }
        void start(string id)
        {
            if (!pool.ContainsKey(id)) return;
            var d = pool[id];
            if (d.State == EnumDownloadState.Downloading || d.State == EnumDownloadState.Processing) return;
            pool[id].State = EnumDownloadState.Waiting;
        }
        void remove(string id)
        {
            if (!pool.ContainsKey(id)) return;
            var d = pool[id];
            d.StopDownload();
            pool.Remove(id);
        }
        #endregion
        #region public control methods
        public bool StartByTag(string[] taglist)
        {
            startSpin();
            bool hasWork = false;
            foreach (var tag in taglist)
            {
                if (!tags.ContainsKey(tag))
                    continue;
                hasWork = true;
                foreach (var id in tags[tag])
                {
                    start(id);
                }
            }
            if (hasWork)
                startSpin();
            return hasWork;
        }
        public void StopByTag(string[] taglist)
        {
            foreach (var tag in taglist)
            {
                if (!tags.ContainsKey(tag))
                    continue;
                foreach (var id in tags[tag])
                {
                    stop(id);
                }
            }
        }
        public void Add(Downloader downloader)
        {
            if (downloader.Info != null && pool.ContainsKey(downloader.Info.Id))
                return;
            pool.Add(downloader.Info.Id, downloader);
            if (downloader.Info.Tag != null)
            {
                if (!tags.ContainsKey(downloader.Info.Tag))
                    tags.Add(downloader.Info.Tag, new List<string>());
                tags[downloader.Info.Tag].Add(downloader.Info.Id);
            }
        }
        public void RemoveByTag(string[] taglist)
        {
            foreach (var tag in taglist)
            {
                if (!tags.ContainsKey(tag)) continue;
                foreach (var id in tags[tag])
                {
                    remove(id);
                }
                tags.Remove(tag);
            }
        }
        public Downloader GetById(string id)
        { return pool.Values.FirstOrDefault(x => x.Id == id); }
        #endregion
        #region concurrency management
        void startSpin()
        {
            if (!dt.IsEnabled)
            {
                dt.Start();
                MessageBus.Instance.Publish(new MsgSetBusy(this, true));
            }
        }
        void endSpin()
        {
            if (dt.IsEnabled)
            {
                dt.Stop();
                MessageBus.Instance.Publish(new MsgSetBusy(this, false));

            }
        }
        //pick next item to download
        bool isPicking;
        void pick()
        {
            if (isPicking)
                return;
            isPicking = true;
            var downloadingCount = pool.Count(x => x.Value.State == EnumDownloadState.Downloading);
            int priority = 0;
            if (downloadingCount > 0)
            {
                priority = pool.Select(x => x.Value).
                    Where(x => x.State == EnumDownloadState.Downloading)
                    .Min(x => x.Info.Priority);
            }
            List<Downloader> waiters = pool.Values
                .Where(x => x.State == EnumDownloadState.Waiting
                    && x.CanDownload
                    && x.Info.Priority >= priority)
                .OrderByDescending(x => x.Info.Priority)
                .ToList();
            if (waiters.Count > 0)
            {
                while (downloadingCount < maxThread)
                {
                    if (waiters.Count == 0)
                        break;
                    var d = waiters[0];
                    waiters.RemoveAt(0);
                    downloadingCount++;
                    d.StartDownload();
                }
            }
            var completeList = pool.Values.Where(x => x.State >= EnumDownloadState.Processing).ToList();
            foreach (var item in completeList)
            {
                remove(item.Info.Id);
            }
            if (downloadingCount == 0 && waiters.Count == 0)
            {
                endSpin();
            }
            isPicking = false;
        }
        #endregion
    }
}
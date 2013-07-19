using Artwork.DataBus;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
namespace Jean_Doe.Common
{
    public enum EnumPlayNextMode
    {
        [Description("顺序")]
        Sequential,
        [Description("随机")]
        Random,
        [Description("单曲循环")]
        Repeat,
    }
    public class Mp3Player
    {
        public static event EventHandler<TimeChangedEventArgs> TimeChanged;
        public static event EventHandler<SongChangedEventArgs> SongChanged;
        private static double refreshInterval = 1000.0;
        public static double RefreshInterval
        {
            get { return refreshInterval; }
        }
        static System.Timers.Timer timer = null;
        static bool isPlaying;
        public static bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
        }
        public static double CurrentTime
        {
            get
            {
                return proxy.GetCurrentTime();
            }
            set
            {
                proxy.SetCurrentTime(value);
            }
        }
        static double totalTime = 0.0;
        static public double TotalTime
        {
            get { return totalTime; }
            set
            {
                totalTime = value; 
                UIHelper.RunOnUI(() =>
                {
                    SongChanged(null, new SongChangedEventArgs { Total = value, Id = _id });
                });
            }
        }
        static IMp3Player proxy;
        static Mp3Player()
        {
            var pipeFactory = new ChannelFactory<IMp3Player>(
                  new NetNamedPipeBinding(),
                  new EndpointAddress(
                  "net.pipe://localhost/Mp3Player"));
            proxy = pipeFactory.CreateChannel();
            timer = new System.Timers.Timer(RefreshInterval);
            timer.Elapsed += timer_Tick;
            Task.Run(() => timer.Start());
            //ctx = ZmqContext.Create();
            //sender = ctx.CreateSocket(SocketType.REQ);
            //sender.Connect("tcp://127.0.0.1:" + DataBus.Get("port1").ToString());
            //receiver = ctx.CreateSocket(SocketType.SUB);
            //receiver.Connect("tcp://127.0.0.1:" + DataBus.Get("port2").ToString());
            //receiver.SubscribeAll();
            //receive();
        }
        //static void receive()
        //{
        //    Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            var cmd = receiver.Receive(Encoding.UTF8);
        //            if (cmd.StartsWith("song_changed ") && SongChanged != null)
        //            {
        //                totalTime = double.Parse(cmd.Substring("song_changed ".Length));
        //                UIHelper.RunOnUI(() =>
        //                {
        //                    SongChanged(null, new SongChangedEventArgs { Total = totalTime, Id = _id });
        //                });
        //            }
        //        }
        //    });
        //}
        static bool isRequestingNext = false;
        public static void Exit()
        {
            Task.Run(() => proxy.Exit());
        }
        public static void Next()
        {
            if (isRequestingNext) return;
            isRequestingNext = true;
            var msg = new MsgRequestNextSong();
            Artwork.MessageBus.MessageBus.Instance.Publish(msg);
            if (!string.IsNullOrEmpty(msg.Next))
            {
                play(msg.Next, msg.Id);
            }
            isRequestingNext = false;
        }
        public static void PauseResume()
        {
            if (isPlaying)
            {
                proxy.Pause();
                isPlaying = false;
            }
            else
            {
                TotalTime = proxy.Play();
                isPlaying = true;
            }
        }
        public static void Play(string filepath, string id)
        {
            if (!System.IO.File.Exists(filepath))
                return;
            if (_id == id)
            {
                PauseResume();
            }
            else
            {
                play(filepath, id);
            }
        }
        static void timer_Tick(object sender, EventArgs e)
        {
            var cur = CurrentTime;
            if (totalTime > 0 && cur >= totalTime)
                Next();
            UIHelper.RunOnUI(() =>
            {
                if (TimeChanged != null)
                    TimeChanged(null, new TimeChangedEventArgs { Current = CurrentTime });
            });

        }
        //static ZmqContext ctx;
        //static ZmqSocket sender;
        //static ZmqSocket receiver;
        //static String Send(string msg)
        //{
        //    try
        //    {
        //        sender.Send(msg, Encoding.UTF8);
        //        return sender.Receive(Encoding.UTF8, TimeSpan.FromMilliseconds(1000));
        //    }
        //    catch (System.Exception)
        //    {
        //        return "fail";
        //    }
        //}
        static void play(string filepath, string id)
        {
            if (isPlaying)
            {
                proxy.Pause();
            }
            isPlaying = false;
            bool ok = proxy.Initialize(filepath);
            if (!ok) return;
            _id = id;
            TotalTime = proxy.Play();
            isPlaying = true;
        }
        static string _id;
    }
    public class TimeChangedEventArgs : EventArgs
    {
        public double Current { get; set; }
    }
    public class SongChangedEventArgs : EventArgs
    {
        public double Total { get; set; }
        public string Id { get; set; }
    }
    public class MsgRequestNextSong
    {
        public string Next { get; set; }
        public string Id { get; set; }
    }
}

using Artwork.DataBus;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using ZMQ;
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
                double res = 0;
                double.TryParse(Send("get_cur_time"), out res);
                return res;
            }
            set
            {
                Send("set_cur_time " + value.ToString());
            }
        }
        static double totalTime = 0.0;
        static Mp3Player()
        {
            timer = new System.Timers.Timer(RefreshInterval);
            timer.Elapsed += timer_Tick;
            Task.Run(() => timer.Start());
            ctx = new Context();
            sender = ctx.Socket(SocketType.REQ);
            sender.Connect("tcp://127.0.0.1:" + DataBus.Get("port1").ToString());
            receiver = ctx.Socket(SocketType.SUB);
            receiver.Connect("tcp://127.0.0.1:" + DataBus.Get("port2").ToString());
            receiver.Subscribe("", Encoding.UTF8);
            receive();
        }
        static void receive()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var cmd = receiver.Recv(Encoding.UTF8);
                    if (cmd.StartsWith("song_changed ") && SongChanged != null)
                    {
                        totalTime = double.Parse(cmd.Substring("song_changed ".Length));
                        UIHelper.RunOnUI(() =>
                        {
                            SongChanged(null, new SongChangedEventArgs { Total = totalTime, Id = _id });
                        });
                    }
                }
            });
        }
        static bool isRequestingNext = false;
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
                if (Send("pause") == "ok")
                    isPlaying = false;
            }
            else
            {
                if (Send("play") == "ok")
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
        static bool inChange = false;
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
        static Context ctx;
        static Socket sender;
        static Socket receiver;
        static String Send(string msg)
        {
            try
            {
                sender.Send(msg, Encoding.UTF8);
                return sender.Recv(Encoding.UTF8, 1000);
            }
            catch (System.Exception)
            {
                return "fail";
            }
        }
        static void play(string filepath, string id)
        {
            if (isPlaying)
            {
                Send("pause");
            }
            isPlaying = false;
            var res = Send("init " + filepath);
            if (res == "fail") return;
            _id = id;
            res = Send("play");
            isPlaying = res == "ok";
        }

        static string _id;

        static WaveStream CreateInputStream(string fileName)
        {
            WaveStream mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }
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

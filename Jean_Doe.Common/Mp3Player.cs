using NAudio.Wave;
using System;
using System.ComponentModel;
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
        static IWavePlayer device = new WaveOut { DesiredLatency = 400, NumberOfBuffers = 4 };
        static WaveStream stream;
        public static event EventHandler<TimeChangedEventArgs> TimeChanged;
        public static event EventHandler<SongChangedEventArgs> SongChanged;
        private static double refreshInterval = 1000.0;
        public static double RefreshInterval
        {
            get { return refreshInterval; }
        }
        static Timer timer = null;
        public static bool IsPlaying { get { return device != null && device.PlaybackState == PlaybackState.Playing; } }
        public static TimeSpan CurrentTime
        {
            get { return stream != null ? stream.CurrentTime : TimeSpan.Zero; }
            set
            {
                if (stream != null)
                    stream.CurrentTime = value;
            }
        }
        static Mp3Player()
        {
            timer = new Timer(RefreshInterval);
            timer.Elapsed += timer_Tick;
            Task.Run(() => timer.Start());
        }
        public static void Next()
        {
            var msg = new MsgRequestNextSong();
            Artwork.MessageBus.MessageBus.Instance.Publish(msg);
            if (!string.IsNullOrEmpty(msg.Next))
            {
                _id = msg.Id;
                play(msg.Next);
            }
        }
        public static void PauseResume()
        {
            if (stream == null) return;
            if (stream.Length < stream.Position)
            {
                CurrentTime = TimeSpan.Zero;
            }
            else if (IsPlaying)
            {
                device.Pause();
            }
            else
            {
                device.Play();
                if (SongChanged != null && stream != null)
                    SongChanged(null, new SongChangedEventArgs { Total = stream.TotalTime, Id = _id });
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
                _id = id;
                play(filepath);
            }
        }
        static void timer_Tick(object sender, EventArgs e)
        {
            if (stream == null)
                return;
            if (stream.Length < stream.Position)
            {
                Next();
            }
            if (TimeChanged != null)
                UIHelper.RunOnUI(() =>
                {
                    TimeChanged(null, new TimeChangedEventArgs { Current = stream.CurrentTime });
                });

        }

        static void play(string filepath)
        {
            if (IsPlaying)
            {
                device.Pause();
            }
            try
            {
                stream = CreateInputStream(filepath);
                device.Init(stream);
                if (SongChanged != null && stream != null)
                    SongChanged(null, new SongChangedEventArgs { Total = stream.TotalTime, Id = _id });
            }
            catch (Exception)
            {
                return;
            }
            device.Play();
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
        public TimeSpan Current { get; set; }
    }
    public class SongChangedEventArgs : EventArgs
    {
        public TimeSpan Total { get; set; }
        public string Id { get; set; }
    }
    public class MsgRequestNextSong
    {
        public string Next { get; set; }
        public string Id { get; set; }
    }
}

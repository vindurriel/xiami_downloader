using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        static IWavePlayer waveOutDevice = new WaveOut();
        static WaveStream waveStream;
        public static event EventHandler<TimeChangedEventArgs> TimeChanged;
        public static event EventHandler<SongChangedEventArgs> SongChanged;
        private static double refreshInterval = 1000.0;

        public static double RefreshInterval
        {
            get { return refreshInterval; }
            set { refreshInterval = value; }
        }
        static DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(RefreshInterval) };
        public static bool IsPlaying { get { return waveOutDevice != null && waveOutDevice.PlaybackState == PlaybackState.Playing; } }
        public static TimeSpan CurrentTime
        {
            get { return waveStream != null ? waveStream.CurrentTime : TimeSpan.Zero; }
            set
            {
                if (waveStream != null)
                    waveStream.CurrentTime = value;
            }
        }
        static Mp3Player()
        {
            timer.Tick += timer_Tick;
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
            if (waveStream == null) return;
            if (waveStream.Length <= waveStream.Position)
            {
                CurrentTime = TimeSpan.Zero;
            }
            else if (IsPlaying)
            {
                waveOutDevice.Pause();
                timer.Stop();
            }
            else
            {
                timer.Start();
                waveOutDevice.Play();
                if (SongChanged != null && waveStream != null)
                    SongChanged(null, new SongChangedEventArgs { Total = waveStream.TotalTime, Id = _id });
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
            if (waveStream.Length <= waveStream.Position)
            {
                Next();
            }
            if (TimeChanged != null && waveStream != null)
                TimeChanged(null, new TimeChangedEventArgs { Current = waveStream.CurrentTime });
        }

        static void play(string filepath)
        {
            if (IsPlaying)
            {
                waveOutDevice.Pause();
                timer.Stop();
            }
            try
            {
                waveStream = CreateInputStream(filepath);
                waveOutDevice.Init(waveStream);
                if (SongChanged != null && waveStream != null)
                    SongChanged(null, new SongChangedEventArgs { Total = waveStream.TotalTime, Id = _id });
            }
            catch (Exception)
            {
                return;
            }
            waveOutDevice.Play();
            timer.Start();
        }
        static string _id = null;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using System.Windows.Threading;
using System.ComponentModel;
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
        static DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };

        public static string FilePath { get; set; }
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


        static void timer_Tick(object sender, EventArgs e)
        {
            if (waveStream.Length<=waveStream.Position)
            {
                var msg=new MsgRequestNextSong();
                Artwork.MessageBus.MessageBus.Instance.Publish(msg);
                if (!string.IsNullOrEmpty(msg.Next))
                    Next(msg.Next);
            }
            if (TimeChanged != null && waveStream!=null)
                TimeChanged(null, new TimeChangedEventArgs { Total = TimeSpan.Zero, Current = waveStream.CurrentTime });
        }
        public static void Next(string filepath)
        {
            if (!System.IO.File.Exists(filepath))
                return;
            play(filepath);
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
                if (TimeChanged != null && waveStream != null)
                    TimeChanged(null, new TimeChangedEventArgs { Total = waveStream.TotalTime, IsNewSong = true });
            }
            catch (Exception)
            {
                return;
            }
            waveOutDevice.Play();
            timer.Start();
            FilePath = filepath;
        }
        public static void PlayPause(string filepath)
        {
            if (!System.IO.File.Exists(filepath))
                return;
            if (filepath == FilePath)
            {
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
                }
            }
            else
            {
                play(filepath);
            }
        }
        static WaveStream CreateInputStream(string fileName)
        {
            WaveStream mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }
        public class TimeChangedEventArgs : EventArgs
        {
            public TimeSpan Total { get; set; }
            public TimeSpan Current { get; set; }
            public bool IsNewSong { get; set; }
        }
    }
    public class MsgRequestNextSong
    {
        public string Next { get; set; }
    }
}

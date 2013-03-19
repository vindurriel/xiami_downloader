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
        [Description("随机")]
        Stop,
    }
    public class Mp3Player
    {
        public static EnumPlayNextMode PlayNextMode { get; set; }
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
            Global.ListenToEvent("PlayNextMode", (s) => { 
                var mode=EnumPlayNextMode.Sequential;
                Enum.TryParse(s,out mode);
                PlayNextMode = mode;
            });

        }


        static void timer_Tick(object sender, EventArgs e)
        {
            if (waveStream.Length<=waveStream.Position)
            {
                CurrentTime = TimeSpan.Zero;
            }
            if (TimeChanged != null && waveStream!=null)
                TimeChanged(null, new TimeChangedEventArgs { Total = TimeSpan.Zero, Current = waveStream.CurrentTime });
        }
        public static void PlayPause(string filepath)
        {

            if (filepath == FilePath)
            {
                if (IsPlaying)
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
                        TimeChanged(null, new TimeChangedEventArgs { Total = waveStream.TotalTime,IsNewSong=true });
                }
                catch (Exception)
                {
                    return;
                }
                waveOutDevice.Play();
                timer.Start();
                FilePath = filepath;
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
}

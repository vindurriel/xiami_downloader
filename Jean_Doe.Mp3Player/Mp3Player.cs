using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jean_Doe.Common;
namespace Jean_Doe.Mp3Player
{
    public class Mp3Player : IMp3Player
    {

        public double Play()
        {
            device.Play();
            return stream.TotalTime.TotalMilliseconds;
        }

        public void Pause()
        {
            device.Pause();
        }

        public void Exit()
        {
            stream.Dispose();
            device.Dispose();
            Environment.Exit(0);
        }
        public void SetCurrentTime(double t)
        {
            CurrentTime = t;
        }
        public double GetCurrentTime()
        {
            return CurrentTime;
        }
        public bool Initialize(string fileName)
        {
            bool res = false;
            try
            {
                if (stream != null)
                    stream.Dispose();
                stream = createInputStream(fileName);
                device.Init(stream);
                res = true;
            }
            catch (Exception e)
            {
                log(e);
            }
            return res;
        }

        WaveStream createInputStream(string fileName)
        {
            WaveStream mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }
        internal double CurrentTime
        {
            get { return stream != null ? stream.CurrentTime.TotalMilliseconds : 0.0; }
            set
            {
                if (stream != null)
                    stream.CurrentTime = TimeSpan.FromMilliseconds(value);
            }
        }
        IWavePlayer device = new WaveOut { DesiredLatency = 400, NumberOfBuffers = 4 };
        WaveStream stream;
        static object _logLock = new object();
        static void log(Exception msg)
        {
            lock (_logLock)
            {
                Console.WriteLine(msg);
                Logger.Error(msg);
            }
        }

    }
}

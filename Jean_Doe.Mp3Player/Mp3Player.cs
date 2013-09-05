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
            try
            {
                device.Pause();
                device.Play();
                return stream.TotalTime.TotalMilliseconds;
            }
            catch (Exception e)
            {
                log(e);
                log(new Exception(f));
                return 0.0;
            }

        }

        public void Pause()
        {
            try
            {
                device.Pause();
            }
            catch (Exception e)
            {
                log(e);
            }
        }

        public void Exit()
        {
            try
            {
                device.Stop();
                device.Dispose();
                if (stream != null)
                    stream.Dispose();
                Program.Exit();
            }
            catch (Exception e)
            {
                log(e);
            }
        }
        public void SetCurrentTime(double t)
        {
            try
            {
                CurrentTime = t;
            }
            catch (Exception e)
            {
                log(e);
            }
        }
        public double GetCurrentTime()
        {
            try
            {
                return CurrentTime;
            }
            catch (Exception e)
            {
                return 0.0;
            }
        }
        string f;
        public bool Initialize(string fileName)
        {
            bool res = false;
            try
            {
                f = fileName;
                if (stream != null)
                    stream.Dispose();
                stream = createInputStream(fileName);
                device.Stop();
                stream.Position = 0;
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
            var mp3Reader = new MediaFoundationReader(fileName);
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

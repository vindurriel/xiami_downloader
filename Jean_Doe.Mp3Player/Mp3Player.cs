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
            Exception x = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    device.Play();
                    return stream.TotalTime.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    x = e;
                    Initialize(f);
                }
            }
            log(x);
            log(new Exception(f));
            return Double.PositiveInfinity;
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
            catch (Exception)
            {
                return 0.0;
            }
        }
        string f;
        public bool Initialize(string fileName)
        {
            try
            {
                f = fileName;
                var s = createInputStream(fileName);
                if (s == null)
                    return false;
                device.Stop();
                device.Init(s);
                if (stream != null)
                    stream.Dispose();
                stream = s;
                return true;
            }
            catch (Exception e)
            {
                log(e);
                return false;
            }
        }

        WaveStream createInputStream(string fileName)
        {
            Exception e = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return new WaveChannel32(new MediaFoundationReader(fileName));
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }
            log(e);
            return null;
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
            if (msg == null) return;
            lock (_logLock)
            {
                Console.WriteLine(msg);
                Logger.Error(msg);
            }
        }

    }
}

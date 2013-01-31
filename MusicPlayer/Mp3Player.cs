using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
namespace MusicPlayer
{
    public class Mp3Player
    {
        static IWavePlayer waveOutDevice = new WaveOut();
        public static string FilePath { get; set; }
        public static bool IsPlaying { get { return waveOutDevice != null && waveOutDevice.PlaybackState == PlaybackState.Playing; } }
        static Mp3Player()
        {
        }
        public static void PlayPause(string filepath)
        {

            if (filepath == FilePath)
            {
                if (IsPlaying)
                {
                    waveOutDevice.Pause();
                }
                else
                    waveOutDevice.Play();
            }
            else
            {
                if (IsPlaying)
                {
                    waveOutDevice.Pause();
                }
                try
                {
                    waveOutDevice.Init(CreateInputStream(filepath));
                }
                catch (Exception)
                {
                    return;
                }
                waveOutDevice.Play();
                FilePath = filepath;
            }
        }
        static WaveStream CreateInputStream(string fileName)
        {
            WaveStream mp3Reader = new Mp3FileReader(fileName);
            return  new WaveChannel32(mp3Reader);
        }
    }
}

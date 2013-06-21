using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZMQ;
namespace Jean_Doe.Mp3Player
{
    class Program
    {
        static IWavePlayer device = new WaveOut { DesiredLatency = 400, NumberOfBuffers = 4 };
        static WaveStream stream;
        static Context ctx;
        static Socket sender;
        static Socket receiver;
        static void Send(string msg)
        {
            try
            {
                sender.Send(msg, Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                log(e.Message);
            }
        }
        static object _logLock = new object();
        static void log(string msg)
        {
            lock (_logLock)
            {
                Console.WriteLine(msg);
                System.IO.File.AppendAllLines("xiami_player.log", new string[] { DateTime.Now.ToString() + " " + msg });
            }
        }
        static void suicide()
        {
            Task.Run(() =>
            {
                while (!parent.HasExited)
                {
                    Thread.Sleep(500);
                }
                Environment.Exit(0);
            });
        }
        static Process parent;
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                log("wrong usage: xiami_player.exe PARENT_PROCESS_ID");
                Environment.Exit(-1);
            }
            parent = Process.GetProcessById(int.Parse(args[0]));
            suicide();
            Console.WriteLine(string.Format("{0} {1} {2}", args[0], args[1], args[2]));
            ctx = new Context();
            receiver = ctx.Socket(SocketType.REP);
            receiver.Bind("tcp://*:" + args[1]);
            sender = ctx.Socket(SocketType.PUB);
            sender.Bind("tcp://*:" + args[2]);
            while (true)
            {
                string cmd = receiver.Recv(Encoding.UTF8);
                string res = "ok";
                switch (cmd)
                {
                    case "pause":
                        device.Pause();
                        break;
                    case "play":
                        device.Play();
                        Send("song_changed " + stream.TotalTime.TotalMilliseconds.ToString());
                        break;
                    case "get_cur_time":
                        res = CurrentTime.ToString();
                        break;
                    default:
                        if (cmd.StartsWith("set_cur_time "))
                        {
                            CurrentTime = double.Parse(cmd.Substring("set_cur_time ".Length));
                        }
                        else if (cmd.StartsWith("init "))
                        {
                            var filename = cmd.Substring(4);
                            if (stream != null)
                                stream.Dispose();
                            stream = createInputStream(filename);
                            if (stream == null)
                                res = "fail";
                            else
                                device.Init(stream);
                        }
                        break;
                }
                if (cmd != "get_cur_time" && !cmd.StartsWith("set_cur_time"))
                    log(cmd);
                receiver.Send(res, Encoding.UTF8);
            }
        }

        static void OnParentExited(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        public static double CurrentTime
        {
            get { return stream != null ? stream.CurrentTime.TotalMilliseconds : 0.0; }
            set
            {
                if (stream != null)
                    stream.CurrentTime = TimeSpan.FromMilliseconds(value);
            }
        }
        static WaveStream createInputStream(string fileName)
        {
            try
            {
                WaveStream mp3Reader = new Mp3FileReader(fileName);
                return new WaveChannel32(mp3Reader);
            }
            catch (System.Exception e)
            {
                log(e.Message);
                return null;
            }
        }
    }
}

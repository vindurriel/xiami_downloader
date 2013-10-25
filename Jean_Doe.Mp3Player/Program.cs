using Jean_Doe.Common;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Jean_Doe.Mp3Player
{
    class Program
    {
        static Process parent;
        static ServiceHost host;
        public static void Exit()
        {
            if (host != null)
                host.Close();
            Environment.Exit(0);
        }
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    throw new ArgumentException("wrong usage: xiami_player.exe PARENT_PROCESS_ID");
                }
                parent = Process.GetProcessById(int.Parse(args[0]));
                host = new ServiceHost(typeof(Mp3Player), new Uri[] { new Uri("net.pipe://localhost/"+args[0]) });
                host.AddServiceEndpoint(typeof(IMp3Player), new NetNamedPipeBinding(), "Mp3Player");
                host.Open();
                while (!parent.HasExited)
                {
                    Thread.Sleep(500);
                }
                Exit();
            }
            catch (Exception e)
            {
                Jean_Doe.Common.Logger.Error(e);
            }
        }
    }
}

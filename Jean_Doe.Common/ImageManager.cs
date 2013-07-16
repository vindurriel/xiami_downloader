using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
namespace Jean_Doe.Common
{
    public class ImageManager
    {
        public enum EnumImageState { New, Downloading, Cached, Error }
        static Dictionary<string, ImageHolder> images = new Dictionary<string, ImageHolder>();
        static BitmapImage getBitmap(byte[] bytes)
        {
            var byteStream = new System.IO.MemoryStream(bytes);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = byteStream;
            bi.EndInit();
            byteStream.Close();
            bi.Freeze();
            return bi;
        }
        class ImageHolder
        {
            public byte[] Data = null;
            public string Id { get; set; }
            public HashSet<Action<BitmapImage>> ReadyActions = new HashSet<Action<BitmapImage>>();
            public EnumImageState State = EnumImageState.New;
            public void Fire()
            {
                BitmapImage image = getBitmap(Data);
                foreach (var action in ReadyActions)
                {
                    call(action, image);
                }
                ReadyActions.Clear();
            }
        }
        static void call(Action<BitmapImage> a, BitmapImage bi)
        {
            if (a != null)
                UIHelper.RunOnUI(() => a(bi));
        }
        public static void Get(string id, string url, Action<BitmapImage> readyAction)
        {
            string localFile = Path.Combine(Global.BasePath, "cache", id);
            //state checking
            if (!images.ContainsKey(id))
            {
                images[id] = new ImageHolder
                {
                    Id = id,
                    State = File.Exists(localFile) ? EnumImageState.Cached : EnumImageState.New
                };
                if (readyAction != null)
                    images[id].ReadyActions.Add(readyAction);
            }
            var img = images[id];
            switch (img.State)
            {
                case EnumImageState.New:
                    Task.Run(() =>
                    {
                        try
                        {
                            img.State = EnumImageState.Downloading;
                            new WebClient().DownloadFile(url, localFile);
                            img.Data = File.ReadAllBytes(localFile);
                            img.Fire();
                            img.State = EnumImageState.Cached;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    });
                    break;
                case EnumImageState.Downloading:
                    if (!img.ReadyActions.Contains(readyAction))
                        img.ReadyActions.Add(readyAction);
                    break;
                case EnumImageState.Cached:
                    try
                    {
                        if (img.Data == null)
                            img.Data = File.ReadAllBytes(localFile);
                        call(readyAction, getBitmap(img.Data));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

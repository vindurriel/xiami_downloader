using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Threading;
using System.Collections.Specialized;
namespace Jean_Doe.Downloader
{
    /// <summary> 
    /// 记录下载的字节位置 
    /// </summary> 
    public class DownLoadState
    {
        private string _FileName;
        private string _AttachmentName;
        private int _Position;
        private string _RequestURL;
        private string _ResponseURL;
        private int _Length;
        private byte[] _Data;
        public string FileName
        {
            get
            {
                return _FileName;
            }
        }
        public int Position
        {
            get
            {
                return _Position;
            }
        }
        public int Length
        {
            get
            {
                return _Length;
            }
        }

        public string AttachmentName
        {
            get
            {
                return _AttachmentName;
            }
        }
        public string RequestURL
        {
            get
            {
                return _RequestURL;
            }
        }
        public string ResponseURL
        {
            get
            {
                return _ResponseURL;
            }
        }

        public byte[] Data
        {
            get
            {
                return _Data;
            }
        }
        internal DownLoadState(string RequestURL, string ResponseURL, string FileName, string AttachmentName, int Position, int Length, byte[] Data)
        {
            this._FileName = FileName;
            this._RequestURL = RequestURL;
            this._ResponseURL = ResponseURL;
            this._AttachmentName = AttachmentName;
            this._Position = Position;
            this._Data = Data;
            this._Length = Length;
        }
        internal DownLoadState(string RequestURL, string ResponseURL, string FileName, string AttachmentName, int Position, int Length, ThreadCallbackHandler tch)
        {
            this._RequestURL = RequestURL;
            this._ResponseURL = ResponseURL;
            this._FileName = FileName;
            this._AttachmentName = AttachmentName;
            this._Position = Position;
            this._Length = Length;
            this._ThreadCallback = tch;
        }
        internal DownLoadState(string RequestURL, string ResponseURL, string FileName, string AttachmentName, int Position, int Length)
        {
            this._RequestURL = RequestURL;
            this._ResponseURL = ResponseURL;
            this._FileName = FileName;
            this._AttachmentName = AttachmentName;
            this._Position = Position;
            this._Length = Length;
        }
        private ThreadCallbackHandler _ThreadCallback;
        public HttpWebClient httpWebClient
        {
            get
            {
                return this._hwc;
            }
            set
            {
                this._hwc = value;
            }
        }
        internal Thread thread
        {
            get
            {
                return _thread;
            }
            set
            {
                _thread = value;
            }
        }
        private HttpWebClient _hwc;
        private Thread _thread;
        // 
        internal void StartDownloadFileChunk()
        {
            if (this._ThreadCallback != null)
            {
                this._ThreadCallback(this._RequestURL, this._FileName, this._Position, this._Length);
                this._hwc.OnThreadProcess(this._thread);
            }
        }
    }
    //委托代理线程的所执行的方法签名一致 
    public delegate void ThreadCallbackHandler(string S, string s, int I, int i);
    //异常处理动作 
    public enum ExceptionActions
    {
        Throw,
        CancelAll,
        Ignore,
        Retry
    }
    /// <summary> 
    /// 包含 Exception 事件数据的类 
    /// </summary> 
    public class ExceptionEventArgs : System.EventArgs
    {
        private System.Exception _Exception;
        private ExceptionActions _ExceptionAction;
        private DownLoadState _DownloadState;
        public DownLoadState DownloadState
        {
            get
            {
                return _DownloadState;
            }
        }
        public Exception Exception
        {
            get
            {
                return _Exception;
            }
        }
        public ExceptionActions ExceptionAction
        {
            get
            {
                return _ExceptionAction;
            }
            set
            {
                _ExceptionAction = value;
            }
        }
        internal ExceptionEventArgs(System.Exception e, DownLoadState DownloadState)
        {
            this._Exception = e;
            this._DownloadState = DownloadState;
        }
    }
    /// <summary> 
    /// 包含 DownLoad 事件数据的类 
    /// </summary> 
    public class DownLoadEventArgs : System.EventArgs
    {
        private DownLoadState _DownloadState;
        public DownLoadState DownloadState
        {
            get
            {
                return _DownloadState;
            }
        }
        public DownLoadEventArgs(DownLoadState DownloadState)
        {
            this._DownloadState = DownloadState;
        }
    }
    public class ThreadProcessEventArgs : System.EventArgs
    {
        private Thread _thread;
        public Thread thread
        {
            get
            {
                return this._thread;
            }
        }
        public ThreadProcessEventArgs(Thread thread)
        {
            this._thread = thread;
        }
    }
    /// <summary> 
    /// 支持断点续传多线程下载的类 
    /// </summary> 
    public class HttpWebClient
    {
        private static object _SyncLockObject = new object();
        public delegate void DataReceiveEventHandler(HttpWebClient Sender, DownLoadEventArgs e);
        public event DataReceiveEventHandler DataReceive; //接收字节数据事件
        public delegate void ExceptionEventHandler(HttpWebClient Sender, ExceptionEventArgs e);
        public event ExceptionEventHandler ExceptionOccurrs; //发生异常事件
        public delegate void ThreadProcessEventHandler(HttpWebClient Sender, ThreadProcessEventArgs e);
        public event ThreadProcessEventHandler ThreadProcessEnd; //发生多线程处理完毕事件

        private int _FileLength; //下载文件的总大小
        public int FileLength
        {
            get
            {
                return _FileLength;
            }
        }
        /// <summary> 
        /// 分块下载文件 
        /// </summary> 
        /// <param name="Address">URL 地址</param> 
        /// <param name="FileName">保存到本地的路径文件名</param> 
        /// <param name="ChunksCount">块数,线程数</param> 
        public void DownloadFile(string Address, string FileName, int ChunksCount)
        {
            int p = 0; // position 
            int s = 0; // chunk size 
            string a = null;
            HttpWebRequest hwrq;
            HttpWebResponse hwrp = null;
            try
            {
                hwrq = (HttpWebRequest)WebRequest.Create(this.GetUri(Address));
                hwrp = (HttpWebResponse)hwrq.GetResponse();
                long L = hwrp.ContentLength;
                hwrq.Credentials = this.m_credentials;
                L = ((L == -1) || (L > 0x7fffffff)) ? ((long)0x7fffffff) : L; //Int32.MaxValue 该常数的值为 2,147,483,647; 即十六进制的 0x7FFFFFFF
                int l = (int)L;
                this._FileLength = l;
                //    在本地预定空间(竟然在多线程下不用先预定空间) 
                //    FileStream sw = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite); 
                //    sw.Write(new byte[l], 0, l); 
                //    sw.Close(); 
                //    sw = null;
                bool b = (hwrp.Headers["Accept-Ranges"] != null & hwrp.Headers["Accept-Ranges"] == "bytes");
                a = hwrp.Headers["Content-Disposition"]; //attachment 
                if (a != null)
                {
                    a = a.Substring(a.LastIndexOf("filename=") + 9);
                }
                else
                {
                    a = FileName;
                }
                int ss = s;
                if (b)
                {
                    s = l / ChunksCount;
                    if (s < 2 * 64 * 1024) //块大小至少为 128 K 字节 
                    {
                        s = 2 * 64 * 1024;
                    }
                    ss = s;
                    int i = 0;
                    while (l > s)
                    {
                        l -= s;
                        if (l < s)
                        {
                            s += l;
                        }
                        if (i++ > 0)
                        {
                            DownLoadState x = new DownLoadState(Address, hwrp.ResponseUri.AbsolutePath, FileName, a, p, s, new ThreadCallbackHandler(this.DownloadFileChunk));
                            //       单线程下载 
                            //       x.StartDownloadFileChunk();
                            x.httpWebClient = this;
                            //多线程下载 
                            Thread t = new Thread(new ThreadStart(x.StartDownloadFileChunk));
                            //this.OnThreadProcess(t); 
                            t.Start();
                        }
                        p += s;
                    }
                    s = ss;
                    byte[] buffer = this.ResponseAsBytes(Address, hwrp, s, FileName);
                    this.OnThreadProcess(Thread.CurrentThread);
                    //    lock (_SyncLockObject) 
                    //    { 
                    //     this._Bytes += buffer.Length; 
                    //    } 
                }
            }
            catch (Exception e)
            {
                ExceptionActions ea = ExceptionActions.Throw;
                if (this.ExceptionOccurrs != null)
                {
                    DownLoadState x = new DownLoadState(Address, hwrp.ResponseUri.AbsolutePath, FileName, a, p, s);
                    ExceptionEventArgs eea = new ExceptionEventArgs(e, x);
                    ExceptionOccurrs(this, eea);
                    ea = eea.ExceptionAction;
                }
                if (ea == ExceptionActions.Throw)
                {
                    if (!(e is WebException) && !(e is SecurityException))
                    {
                        throw new WebException("net_webclient", e);
                    }
                    throw;
                }
            }
        }
        internal void OnThreadProcess(Thread t)
        {
            if (ThreadProcessEnd != null)
            {
                ThreadProcessEventArgs tpea = new ThreadProcessEventArgs(t);
                ThreadProcessEnd(this, tpea);
            }
        }
        /// <summary> 
        /// 下载一个文件块,利用该方法可自行实现多线程断点续传 
        /// </summary> 
        /// <param name="Address">URL 地址</param> 
        /// <param name="FileName">保存到本地的路径文件名</param> 
        /// <param name="Length">块大小</param> 
        public void DownloadFileChunk(string Address, string FileName, int FromPosition, int Length)
        {
            HttpWebResponse hwrp = null;
            string a = null;
            try
            {
                //this._FileName = FileName; 
                HttpWebRequest hwrq = (HttpWebRequest)WebRequest.Create(this.GetUri(Address));
                //hwrq.Credentials = this.m_credentials; 
                hwrq.AddRange(FromPosition);
                hwrp = (HttpWebResponse)hwrq.GetResponse();
                a = hwrp.Headers["Content-Disposition"]; //attachment 
                if (a != null)
                {
                    a = a.Substring(a.LastIndexOf("filename=") + 9);
                }
                else
                {
                    a = FileName;
                }
                byte[] buffer = this.ResponseAsBytes(Address, hwrp, Length, FileName);
                //   lock (_SyncLockObject) 
                //   { 
                //    this._Bytes += buffer.Length; 
                //   } 
            }
            catch (Exception e)
            {
                ExceptionActions ea = ExceptionActions.Throw;
                if (this.ExceptionOccurrs != null)
                {
                    DownLoadState x = new DownLoadState(Address, hwrp.ResponseUri.AbsolutePath, FileName, a, FromPosition, Length);
                    ExceptionEventArgs eea = new ExceptionEventArgs(e, x);
                    ExceptionOccurrs(this, eea);
                    ea = eea.ExceptionAction;
                }
                if (ea == ExceptionActions.Throw)
                {
                    if (!(e is WebException) && !(e is SecurityException))
                    {
                        throw new WebException("net_webclient", e);
                    }
                    throw;
                }
            }
        }
        internal byte[] ResponseAsBytes(string RequestURL, WebResponse Response, long Length, string FileName)
        {
            string a = null; //AttachmentName 
            int P = 0; //整个文件的位置指针 
            int num2 = 0;
            try
            {
                a = Response.Headers["Content-Disposition"]; //attachment 
                if (a != null)
                {
                    a = a.Substring(a.LastIndexOf("filename=") + 9);
                }
                long num1 = Length; //Response.ContentLength; 
                bool flag1 = false;
                if (num1 == -1)
                {
                    flag1 = true;
                    num1 = 0x10000; //64k 
                }
                byte[] buffer1 = new byte[(int)num1];

                int p = 0; //本块的位置指针
                string s = Response.Headers["Content-Range"];
                if (s != null)
                {
                    s = s.Replace("bytes ", "");
                    s = s.Substring(0, s.IndexOf("-"));
                    P = Convert.ToInt32(s);
                }
                int num3 = 0;
                Stream S = Response.GetResponseStream();
                do
                {
                    num2 = S.Read(buffer1, num3, ((int)num1) - num3);
                    num3 += num2;
                    if (flag1 && (num3 == num1))
                    {
                        num1 += 0x10000;
                        byte[] buffer2 = new byte[(int)num1];
                        Buffer.BlockCopy(buffer1, 0, buffer2, 0, num3);
                        buffer1 = buffer2;
                    }
                    //    lock (_SyncLockObject) 
                    //    { 
                    //     this._bytes += num2; 
                    //    } 
                    if (num2 > 0)
                    {
                        if (this.DataReceive != null)
                        {
                            byte[] buffer = new byte[num2];
                            Buffer.BlockCopy(buffer1, p, buffer, 0, buffer.Length);
                            DownLoadState dls = new DownLoadState(RequestURL, Response.ResponseUri.AbsolutePath, FileName, a, P, num2, buffer);
                            DownLoadEventArgs dlea = new DownLoadEventArgs(dls);
                            //触发事件 
                            this.OnDataReceive(dlea);
                            //System.Threading.Thread.Sleep(100);
                        }
                        p += num2; //本块的位置指针 
                        P += num2; //整个文件的位置指针 
                    }
                    else
                    {
                        break;
                    }
                }
                while (num2 != 0);
                S.Close();
                S = null;
                if (flag1)
                {
                    byte[] buffer3 = new byte[num3];
                    Buffer.BlockCopy(buffer1, 0, buffer3, 0, num3);
                    buffer1 = buffer3;
                }
                return buffer1;
            }
            catch (Exception e)
            {
                ExceptionActions ea = ExceptionActions.Throw;
                if (this.ExceptionOccurrs != null)
                {
                    DownLoadState x = new DownLoadState(RequestURL, Response.ResponseUri.AbsolutePath, FileName, a, P, num2);
                    ExceptionEventArgs eea = new ExceptionEventArgs(e, x);
                    ExceptionOccurrs(this, eea);
                    ea = eea.ExceptionAction;
                }
                if (ea == ExceptionActions.Throw)
                {
                    if (!(e is WebException) && !(e is SecurityException))
                    {
                        throw new WebException("net_webclient", e);
                    }
                    throw;
                }
                return null;
            }
        }
        private void OnDataReceive(DownLoadEventArgs e)
        {
            //触发数据到达事件 
            DataReceive(this, e);
        }
        public byte[] UploadFile(string address, string fileName)
        {
            return this.UploadFile(address, "POST", fileName, "file");
        }
        public string UploadFileEx(string address, string method, string fileName, string fieldName)
        {
            return Encoding.ASCII.GetString(UploadFile(address, method, fileName, fieldName));
        }
        public byte[] UploadFile(string address, string method, string fileName, string fieldName)
        {
            byte[] buffer4;
            FileStream stream1 = null;
            try
            {
                fileName = Path.GetFullPath(fileName);
                string text1 = "---------------------" + DateTime.Now.Ticks.ToString("x");
                string text2 = "application/octet-stream";
                stream1 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                WebRequest request1 = WebRequest.Create(this.GetUri(address));
                request1.Credentials = this.m_credentials;
                request1.ContentType = "multipart/form-data; boundary=" + text1;
                request1.Method = method;
                string[] textArray1 = new string[7] { "--", text1, "\r\nContent-Disposition: form-data; name=\"" + fieldName + "\"; filename=\"", Path.GetFileName(fileName), "\"\r\nContent-Type: ", text2, "\r\n\r\n" };
                string text3 = string.Concat(textArray1);
                byte[] buffer1 = Encoding.UTF8.GetBytes(text3);
                byte[] buffer2 = Encoding.ASCII.GetBytes("\r\n--" + text1 + "\r\n");
                long num1 = 0x7fffffffffffffff;
                try
                {
                    num1 = stream1.Length;
                    request1.ContentLength = (num1 + buffer1.Length) + buffer2.Length;
                }
                catch
                {
                }
                byte[] buffer3 = new byte[Math.Min(0x2000, (int)num1)];
                using (Stream stream2 = request1.GetRequestStream())
                {
                    int num2;
                    stream2.Write(buffer1, 0, buffer1.Length);
                    do
                    {
                        num2 = stream1.Read(buffer3, 0, buffer3.Length);
                        if (num2 != 0)
                        {
                            stream2.Write(buffer3, 0, num2);
                        }
                    }
                    while (num2 != 0);
                    stream2.Write(buffer2, 0, buffer2.Length);
                }
                stream1.Close();
                stream1 = null;
                WebResponse response1 = request1.GetResponse();
                buffer4 = this.ResponseAsBytes(response1);
            }
            catch (Exception exception1)
            {
                if (stream1 != null)
                {
                    stream1.Close();
                    stream1 = null;
                }
                if (!(exception1 is WebException) && !(exception1 is SecurityException))
                {
                    //throw new WebException(SR.GetString("net_webclient"), exception1); 
                    throw new WebException("net_webclient", exception1);
                }
                throw;
            }
            return buffer4;
        }
        private byte[] ResponseAsBytes(WebResponse response)
        {
            int num2;
            long num1 = response.ContentLength;
            bool flag1 = false;
            if (num1 == -1)
            {
                flag1 = true;
                num1 = 0x10000;
            }
            byte[] buffer1 = new byte[(int)num1];
            Stream stream1 = response.GetResponseStream();
            int num3 = 0;
            do
            {
                num2 = stream1.Read(buffer1, num3, ((int)num1) - num3);
                num3 += num2;
                if (flag1 && (num3 == num1))
                {
                    num1 += 0x10000;
                    byte[] buffer2 = new byte[(int)num1];
                    Buffer.BlockCopy(buffer1, 0, buffer2, 0, num3);
                    buffer1 = buffer2;
                }
            }
            while (num2 != 0);
            stream1.Close();
            if (flag1)
            {
                byte[] buffer3 = new byte[num3];
                Buffer.BlockCopy(buffer1, 0, buffer3, 0, num3);
                buffer1 = buffer3;
            }
            return buffer1;
        }
        private NameValueCollection m_requestParameters;
        private Uri m_baseAddress;
        private ICredentials m_credentials = CredentialCache.DefaultCredentials;
        public ICredentials Credentials
        {
            get
            {
                return this.m_credentials;
            }
            set
            {
                this.m_credentials = value;
            }
        }
        public NameValueCollection QueryString
        {
            get
            {
                if (this.m_requestParameters == null)
                {
                    this.m_requestParameters = new NameValueCollection();
                }
                return this.m_requestParameters;
            }
            set
            {
                this.m_requestParameters = value;
            }
        }
        public string BaseAddress
        {
            get
            {
                if (this.m_baseAddress != null)
                {
                    return this.m_baseAddress.ToString();
                }
                return string.Empty;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.m_baseAddress = null;
                }
                else
                {
                    try
                    {
                        this.m_baseAddress = new Uri(value);
                    }
                    catch (Exception exception1)
                    {
                        throw new ArgumentException("value", exception1);
                    }
                }
            }
        }
        private Uri GetUri(string path)
        {
            Uri uri1;
            try
            {
                if (this.m_baseAddress != null)
                {
                    uri1 = new Uri(this.m_baseAddress, path);
                }
                else
                {
                    uri1 = new Uri(path);
                }
                if (this.m_requestParameters == null)
                {
                    return uri1;
                }
                StringBuilder builder1 = new StringBuilder();
                string text1 = string.Empty;
                for (int num1 = 0; num1 < this.m_requestParameters.Count; num1++)
                {
                    builder1.Append(text1 + this.m_requestParameters.AllKeys[num1] + "=" + this.m_requestParameters[num1]);
                    text1 = "&";
                }
                UriBuilder builder2 = new UriBuilder(uri1);
                builder2.Query = builder1.ToString();
                uri1 = builder2.Uri;
            }
            catch (UriFormatException)
            {
                uri1 = new Uri(Path.GetFullPath(path));
            }
            return uri1;
        }
    }
    /// <summary> 
    /// 测试类 
    /// </summary> 
    class AppTest
    {
        int _k = 0;
        int _K = 0;
        static void Main()
        {
            AppTest a = new AppTest();
            HttpWebClient x = new HttpWebClient();

            a._K = 10;
            //订阅 DataReceive　事件 
            x.DataReceive += new HttpWebClient.DataReceiveEventHandler(a.x_DataReceive);
            //订阅 ExceptionOccurrs　事件 
            x.ExceptionOccurrs += new HttpWebClient.ExceptionEventHandler(a.x_ExceptionOccurrs);
            x.ThreadProcessEnd += new HttpWebClient.ThreadProcessEventHandler(a.x_ThreadProcessEnd);
            string F = "http://localhost/download/phpMyAdmin-2.6.1-pl2.zip";
            a._F = F;
            F = "http://localhost/download/jdk-1_5_0_01-windows-i586-p.aa.exe";
            //F = "http://localhost/download/ReSharper1.5.exe";
            //F = "http://localhost/mywebapplications/WebApplication7/WebForm1.aspx"; 
            //F = "http://localhost:1080/test/download.jsp";
            //F = "http://localhost/download/Webcast20050125_PPT.zip"; 
            //F = "http://www.morequick.com/greenbrowsergb.zip"; 
            //F = "http://localhost/download/test_local.rar"; 
            string f = F.Substring(F.LastIndexOf("/") + 1);
            //(new System.Threading.Thread(new System.Threading.ThreadStart(new ThreadProcessState(F, @"E:\temp\" + f, 10, x).StartThreadProcess))).Start();
            x.DownloadFile(F, @"E:\temp\temp\" + f, a._K);
            //  x.DownloadFileChunk(F, @"E:\temp\" + f,15,34556);
            System.Console.ReadLine();
            //  string uploadfile = "e:\\test_local.rar"; 
            //  string str = x.UploadFileEx("http://localhost/phpmyadmin/uploadaction.php", "POST", uploadfile, "file1"); 
            //  System.Console.WriteLine(str); 
            //  System.Console.ReadLine(); 
        }
        string bs = ""; //用于记录上次的位数 
        bool b = false;
        private int i = 0;
        private static object _SyncLockObject = new object();
        string _F;
        string _f;
        private void x_DataReceive(HttpWebClient Sender, DownLoadEventArgs e)
        {
            if (!this.b)
            {
                lock (_SyncLockObject)
                {
                    if (!this.b)
                    {
                        System.Console.Write(System.DateTime.Now.ToString() + " 已接收数据:           ");
                        //System.Console.Write( System.DateTime.Now.ToString() + " 已接收数据:           "); 
                        this.b = true;
                    }
                }
            }
            string f = e.DownloadState.FileName;
            if (e.DownloadState.AttachmentName != null)
                f = System.IO.Path.GetDirectoryName(f) + @"\" + e.DownloadState.AttachmentName;
            this._f = f;
            using (System.IO.FileStream sw = new System.IO.FileStream(f, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                sw.Position = e.DownloadState.Position;
                sw.Write(e.DownloadState.Data, 0, e.DownloadState.Data.Length);
                sw.Close();
            }
            string s = System.DateTime.Now.ToString();
            lock (_SyncLockObject)
            {
                this.i += e.DownloadState.Data.Length;
                System.Console.Write(bs + "\b\b\b\b\b\b\b\b\b\b" + i + " / " + Sender.FileLength + " 字节数据 " + s);
                //System.Console.Write(bs + i + " 字节数据 " + s); 
                this.bs = new string('\b', Digits(i) + 3 + Digits(Sender.FileLength) + s.Length);
            }
        }
        int Digits(int n) //数字所占位数 
        {
            n = System.Math.Abs(n);
            n = n / 10;
            int i = 1;
            while (n > 0)
            {
                n = n / 10;
                i++;
            }
            return i;
        }
        private void x_ExceptionOccurrs(HttpWebClient Sender, ExceptionEventArgs e)
        {
            System.Console.WriteLine(e.Exception.Message);
            //发生异常重新下载相当于断点续传,你可以自己自行选择处理方式 
            HttpWebClient x = new HttpWebClient();
            x.DownloadFileChunk(this._F, this._f, e.DownloadState.Position, e.DownloadState.Length);
            e.ExceptionAction = ExceptionActions.Ignore;
        }
        private void x_ThreadProcessEnd(HttpWebClient Sender, ThreadProcessEventArgs e)
        {
            //if (e.thread.ThreadState == System.Threading.ThreadState.Stopped) 
            if (this._k++ == this._K - 1)
                System.Console.WriteLine("\nend");
        }
    }
}
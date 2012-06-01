//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Artwork.MessageBus.Interfaces;
//using System.Windows;
//using Jean_Doe.MusicControl;
//using MusicPlayer;
//using Artwork.DataBus;
//public class WindowManager : IHandle<MsgChangeWindowState>
//{

//    public static Dictionary<EnumWindows, Window> WindowPool = new Dictionary<EnumWindows, Window>();
//    public WindowManager()
//    {
//        var bus = Artwork.MessageBus.MessageBus.Instance;

//        bus.Subscribe(this);
//    }
//    public static void Register(EnumWindows ew, Window w)
//    {
//        if(WindowPool.ContainsKey(ew))
//            WindowPool[ew] = w;
//        else
//            WindowPool.Add(ew, w);
//    }
//    public void Handle(MsgChangeWindowState message)
//    {
//        var state = message.State;
//        var enumWindow = message.Window;
//        if(!WindowPool.ContainsKey(enumWindow)) return;
//        var window = WindowPool[enumWindow];
//        if(window == null) return;
//        switch(state)
//        {
//            case EnumChangeWindowStateCommand.Close:
//                window.Close();
//                break;
//            case EnumChangeWindowStateCommand.Maximized:
//                var s = window.WindowState;
//                if(s == WindowState.Maximized)
//                {
//                    window.WindowState = WindowState.Normal;
//                }

//                else
//                {
//                    window.SizeToContent = SizeToContent.Manual;
//                    window.WindowState = WindowState.Maximized;
//                }

//                break;
//            case EnumChangeWindowStateCommand.Minimized:
//                window.WindowState = WindowState.Minimized;
//                window.SizeToContent = SizeToContent.Manual;
//                break;
//            default:
//                break;
//        }
//    }
//    static void window_StateChanged(object sender, EventArgs e)
//    {
//        //MessageBox.Show("ds");
//        var win = sender as Window;
//        var state = win.WindowState;
//        switch(state)
//        {
//            case WindowState.Maximized:
//                win.SizeToContent = SizeToContent.Manual;
//                break;
//            case WindowState.Minimized:
//                break;
//            case WindowState.Normal:
//                win.SizeToContent = SizeToContent.WidthAndHeight;
//                break;
//            default:
//                break;
//        }
//    }
//}
//public enum EnumWindows { MainWindow, ConfigWindow}

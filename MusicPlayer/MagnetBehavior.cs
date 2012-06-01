using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
public static class MagnetBehavior
{
    class MagnetHost : IDisposable
    {
        public double Border { get; set; }
        Window window;
        bool topmost = false;
        public MagnetHost(Window win)
        {
            this.window = win;
            topmost = window.Topmost;
            window.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            window.MouseEnter += MainWindow_MouseEnter;
            window.MouseLeave += MainWindow_MouseLeave;
            Border = 2;
            storyboard = new Storyboard() { FillBehavior = FillBehavior.Stop };
            anim = new DoubleAnimation()
            {
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut },
                Duration = TimeSpan.FromMilliseconds(200),
            };
            Storyboard.SetTarget(anim, window);
            storyboard.Children.Add(anim);
            Rim = DetectRim();
            storyboard.Completed += storyboard_Completed;
            win.StateChanged += win_StateChanged;
        }
        void win_StateChanged(object sender, EventArgs e)
        {
            if (window.WindowState == WindowState.Normal)
            {
                slideShow(true);
            }
        }
        void storyboard_Completed(object sender, EventArgs e)
        {
            isPlaying = false;
            hiding = !hiding;
            if (!topmost)
                window.Topmost = Rim != EnumRim.None;
        }
        void MainWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            slideShow();
        }
        void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            Rim = DetectRim();
            slideHide();
        }
        void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rim = DetectRim();
        }
        public void Dispose()
        {
            window.MouseLeftButtonUp -= MainWindow_MouseLeftButtonUp;
            window.MouseEnter -= MainWindow_MouseEnter;
            window.MouseLeave -= MainWindow_MouseLeave;
        }
        Storyboard storyboard;
        System.Drawing.Rectangle screen;
        DoubleAnimation anim;
        double hidingPos;
        double showPos;
        EnumRim DetectRim()
        {
            if (window.WindowState == WindowState.Maximized) return EnumRim.None;
            screen = window.GetScreen().WorkingArea;
            if (window.Left - screen.X < Border) return EnumRim.Left;
            if (window.Top - screen.Y < Border) return EnumRim.Top;
            if (screen.X + screen.Width - window.Left - window.Width < Border) return EnumRim.Right;
            if (screen.Y + screen.Height - window.Top - window.Height < Border) return EnumRim.Bottom;
            return EnumRim.None;
        }
        public event EventHandler RimChanged;
        bool hiding = false;
        private EnumRim rim;
        public EnumRim Rim
        {
            get { return rim; }
            set
            {
                rim = value;
                switch (value)
                {
                    case EnumRim.Top:
                        Storyboard.SetTargetProperty(anim, new PropertyPath("Top"));
                        hidingPos = screen.Y + Border - window.Height;
                        showPos = screen.Y;
                        break;
                    case EnumRim.Bottom:
                        hidingPos = screen.Y + screen.Height - Border;
                        showPos = screen.Y + screen.Height - window.Height;
                        Storyboard.SetTargetProperty(anim, new PropertyPath("Top"));
                        break;
                    case EnumRim.Left:
                        Storyboard.SetTargetProperty(anim, new PropertyPath("Left"));
                        hidingPos = screen.X + Border - window.Width;
                        showPos = screen.X;
                        break;
                    case EnumRim.Right:
                        Storyboard.SetTargetProperty(anim, new PropertyPath("Left"));
                        hidingPos = screen.X + screen.Width - Border;
                        showPos = screen.X + screen.Width - window.Width;
                        break;
                    case EnumRim.None:
                        break;
                    default:
                        break;
                }
                if (RimChanged != null)
                    RimChanged(this, EventArgs.Empty);
            }
        }
        bool isPlaying = false;
        void slideShow(bool fullAnimation = false)
        {
            if (Rim == EnumRim.None) return;
            if (isPlaying) return;
            if (!hiding) return;
            isPlaying = true;
            anim.BeginTime = TimeSpan.FromMilliseconds(0);
            anim.From = hidingPos;
            anim.To = showPos;
            storyboard.Begin();
        }
        void slideHide(bool fullAnimation = false)
        {
            if (Rim == EnumRim.None) return;
            if (isPlaying) return;
            if (hiding) return;
            isPlaying = true;
            anim.BeginTime = TimeSpan.FromMilliseconds(600);
            anim.From = showPos;
            anim.To = hidingPos;
            storyboard.Begin();
        }
    }
    public static void SetMagnetBorder(this Window w, double b)
    {
        if (!dict.ContainsKey(w.GetHashCode())) return;
        dict[w.GetHashCode()].Border = b;
    }
    public static System.Windows.Forms.Screen GetScreen(this Window window)
    {
        return System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
    }
    static Dictionary<int, MagnetHost> dict = new Dictionary<int, MagnetHost>();
    public static void EnableMagnet(this Window window)
    {
        var hash = window.GetHashCode();
        if (dict.ContainsKey(hash)) return;
        var mw = new MagnetHost(window);
        dict.Add(hash, mw);
    }

    public static void DisableMagnet(this Window window)
    {
        var hash = window.GetHashCode();
        if (!dict.ContainsKey(hash)) return;
        dict[hash].Dispose();
        dict.Remove(hash);
    }
    public static void ListenToRimChanged(this Window window, Action<EnumRim> a)
    {
        var hash = window.GetHashCode();
        if (!dict.ContainsKey(hash)) return;
        dict[hash].RimChanged += (s, e) => a(dict[hash].Rim);
    }

}

public enum EnumRim { Top, Left, Right, Bottom, None }

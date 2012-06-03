using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
namespace Jean_Doe.MusicControl
{
    public class ResizeWindow : Window
    {
        public ResizeWindow() { }
        static ResizeWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeWindow), new FrameworkPropertyMetadata(typeof(ResizeWindow)));
            EventManager.RegisterClassHandler(typeof(ResizeWindow), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));

        }

        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeWindow transparentWindow = (ResizeWindow)sender;
            Thumb thumb = e.OriginalSource as Thumb;
            if (thumb != null && transparentWindow.WindowState == WindowState.Normal)
            {
                Rect windowRect = new Rect(transparentWindow.Left, transparentWindow.Top, transparentWindow.ActualWidth, transparentWindow.ActualHeight);
                double maxReducedHeight = Math.Max(0, transparentWindow.ActualHeight - transparentWindow.MinHeight);
                double maxReducedWidth = Math.Max(0, transparentWindow.ActualWidth - transparentWindow.MinWidth);
                double reducedHeight = e.VerticalChange;
                double reducedWidth = e.HorizontalChange;
                if (thumb.Name.Equals("PART_SizeSE"))
                {
                    reducedHeight = Math.Max(reducedHeight, -maxReducedHeight);
                    reducedWidth = Math.Max(reducedWidth, -maxReducedWidth);
                    transparentWindow.Width = Math.Max(transparentWindow.ActualWidth + reducedWidth, transparentWindow.MinWidth);
                    transparentWindow.Height = Math.Max(transparentWindow.ActualHeight + reducedHeight, transparentWindow.MinHeight);
                }
                else if (thumb.Name.Equals("PART_SizeNW"))
                {
                    reducedWidth = Math.Min(reducedWidth, maxReducedWidth);
                    reducedHeight = Math.Min(reducedHeight, maxReducedHeight);
                    windowRect.Y += reducedHeight;
                    windowRect.X += reducedWidth;
                    windowRect.Width -= reducedWidth;
                    windowRect.Height -= reducedHeight;
                    transparentWindow.SetWindowVisualRect(windowRect);
                }
                else if (thumb.Name.Equals("PART_SizeSW"))
                {
                    reducedHeight = Math.Max(reducedHeight, -maxReducedHeight);
                    reducedWidth = Math.Min(reducedWidth, maxReducedWidth);
                    windowRect.X += reducedWidth;
                    windowRect.Width -= reducedWidth;
                    windowRect.Height += reducedHeight;
                    transparentWindow.SetWindowVisualRect(windowRect);
                }
                else if (thumb.Name.Equals("PART_SizeNE"))
                {
                    reducedHeight = Math.Min(reducedHeight, maxReducedHeight);
                    reducedWidth = Math.Max(reducedWidth, -maxReducedWidth);
                    windowRect.Y += reducedHeight;
                    windowRect.Height = transparentWindow.ActualHeight - reducedHeight;
                    windowRect.Width = transparentWindow.ActualWidth + reducedWidth;
                    transparentWindow.SetWindowVisualRect(windowRect);
                }
                else if (thumb.Name.Equals("PART_SizeN"))
                {
                    reducedHeight = Math.Min(reducedHeight, maxReducedHeight);
                    windowRect.Y += reducedHeight;
                    windowRect.Height = transparentWindow.ActualHeight - reducedHeight;
                    transparentWindow.SetWindowVisualRect(windowRect);
                }
                else if (thumb.Name.Equals("PART_SizeS"))
                {
                    reducedHeight = Math.Max(reducedHeight, -maxReducedHeight);
                    transparentWindow.Height = transparentWindow.ActualHeight + reducedHeight;
                }
                else if (thumb.Name.Equals("PART_SizeW"))
                {
                    reducedWidth = Math.Min(reducedWidth, maxReducedWidth);
                    windowRect.X += reducedWidth;
                    windowRect.Width = transparentWindow.ActualWidth - reducedWidth;
                    transparentWindow.SetWindowVisualRect(windowRect);
                }
                else if (thumb.Name.Equals("PART_SizeE"))
                {
                    reducedWidth = Math.Max(reducedWidth, -maxReducedWidth);
                    transparentWindow.Width = transparentWindow.ActualWidth + reducedWidth;
                }
            }
        }

        private void SetWindowVisualRect(Rect rect)
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            Point deviceTopLeft = mainWindowSrc.CompositionTarget.TransformToDevice.Transform(rect.TopLeft);
            Point deviceBottomRight = mainWindowSrc.CompositionTarget.TransformToDevice.Transform(rect.BottomRight);
            NativeMethods.SetWindowPos(mainWindowSrc.Handle,
                                       IntPtr.Zero,
                                       (int)(deviceTopLeft.X),
                                       (int)(deviceTopLeft.Y),
                                       (int)(Math.Abs(deviceBottomRight.X - deviceTopLeft.X)),
                                       (int)(Math.Abs(deviceBottomRight.Y - deviceTopLeft.Y)),
                                       0);
        }
        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        }



        public double BorderWidth
        {
            get { return (double)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderWidthProperty =
            DependencyProperty.Register("BorderWidth", typeof(double), typeof(ResizeWindow), new UIPropertyMetadata(2.0));
    }
}

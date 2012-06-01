using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace MusicPlayer
{
    static class DisplayHelper
    {
        public static void DispatcherInvoke(this FrameworkElement source, Action a)
        {
            source.Dispatcher.BeginInvoke(a);
        }
    }
}

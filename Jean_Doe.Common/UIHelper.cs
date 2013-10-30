using System;

namespace Jean_Doe.Common
{
    public static class UIHelper
    {
        public static void RunOnUI(Action a)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(a);
        }
        public static void WaitOnUI(Action a)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(a);
        }
    }
}

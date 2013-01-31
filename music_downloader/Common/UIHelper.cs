using System;

namespace Jean_Doe.Common
{
    public static class UIHelper
    {
        public static async void RunOnUI(Action a)
        {
            await Windows.UI.Xaml.Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, 
                () => { a(); });
        }
    }
}

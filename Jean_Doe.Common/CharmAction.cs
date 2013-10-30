using System;
using System.ComponentModel;
using System.Windows;

namespace Jean_Doe.Common
{
    public class CharmAction : INotifyPropertyChanged
    {
        private string label;

        public string Label
        {
            get { return label; }
            set { label = value; Notify("Label"); }
        } private string icon;

        public string Icon
        {
            get { return icon; }
            set { icon = value; Notify("Icon"); }
        }

        public Action<object, System.Windows.RoutedEventArgs> Action { get; set; }
        public CharmAction(string l, string i, Action<object, RoutedEventArgs> a)
        {
            Label = l;
            Icon = i;
            Action = a;
            Validate = (s) => { return (s is bool) && (bool)s; };
        }
        public CharmAction(string l, string i, Action<object, RoutedEventArgs> a, Func<object, bool> v=null)
        {
            Label = l;
            Icon = i;
            Action = a;
            Validate = v;
        }
        private bool isActive = true;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; Notify("IsActive"); }
        }

        public Func<object, bool> Validate { get; set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}

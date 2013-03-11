using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
namespace Jean_Doe.Common
{

    public class CharmBarEntity
    {
        private Binding b = new Binding();

        public Binding Binding
        {
            get { return b; }
            set { b = value; }
        }
        public ObservableCollection<CharmAction> Actions = new ObservableCollection<CharmAction>();
    }
    public class CharmAction : INotifyPropertyChanged
    {
        private string label;

        public string Label
        {
            get { return label; }
            set { label = value; Notify("Label"); }
        }

        public Action<object, System.Windows.RoutedEventArgs> Action { get; set; }
        public CharmAction(string l, Action<object, RoutedEventArgs> a)
        {
            Label = l;
            Action = a;
            Validate = (s) => { return (s is bool) && (bool)s; };
        }
        public CharmAction(string l, Action<object, RoutedEventArgs> a, Func<object,bool> v)
        {
            Label = l;
            Action = a;
            Validate = v;
        }
        private bool isActive=true;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; Notify("IsActive"); }
        }

        public Func<object,bool> Validate { get; set; }

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
